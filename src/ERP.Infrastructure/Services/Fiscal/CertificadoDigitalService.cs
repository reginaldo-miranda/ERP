using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ERP.Infrastructure.Services.Fiscal;

public class CertificadoDigitalService : ICertificadoDigitalService
{
    private readonly IApplicationDbContext _context;
    private readonly byte[] _aesKey;

    public CertificadoDigitalService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;

        // Chave AES-256 derivada da configuração ou salt da aplicação
        string segredo = configuration["Fiscal:EncryptionKey"] ?? "ERP_MultiEmpresa_ChaveSegredo_AES256_Fiscal_2026";
        using var sha256 = SHA256.Create();
        _aesKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(segredo));
    }

    public async Task<CertificadoDigitalDto> SalvarCertificadoAsync(
        int empresaId,
        SalvarCertificadoDigitalDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.ArquivoBytes == null || dto.ArquivoBytes.Length == 0)
            throw new ArgumentException("Arquivo do certificado digital não foi fornecido.", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Senha))
            throw new ArgumentException("A senha do certificado digital é obrigatória.", nameof(dto));

        // 1. Tentar carregar para validar se o PFX e a senha estão corretos
        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(dto.ArquivoBytes, dto.Senha, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException($"Não foi possível abrir o certificado digital. Verifique se a senha está correta ou se o arquivo é um .pfx/.p12 válido. Erro: {ex.Message}", ex);
        }

        // 2. Extrair metadados ICP-Brasil
        string thumbprint = cert.Thumbprint;
        DateTime validadeInicio = cert.NotBefore.ToUniversalTime();
        DateTime validadeFim = cert.NotAfter.ToUniversalTime();
        string emissor = cert.Issuer;
        string subject = cert.Subject;

        // Extrair CNPJ/CPF do Subject ou Common Name
        string cnpjCpf = ExtrairCnpjOuCpf(subject);
        string razaoSocial = ExtrairNomeTitular(subject);

        // 3. Criptografar bytes do PFX e a Senha com AES-256
        byte[] arquivoCriptografado = CriptografarBytes(dto.ArquivoBytes);
        string senhaCriptografada = CriptografarTexto(dto.Senha);

        // 4. Salvar ou atualizar no banco
        var existente = await _context.CertificadosDigitais
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId, cancellationToken);

        if (existente == null)
        {
            existente = new CertificadoDigital
            {
                EmpresaId = empresaId,
                NomeArquivo = dto.NomeArquivo,
                ArquivoBytes = arquivoCriptografado,
                SenhaCriptografada = senhaCriptografada,
                ValidadeInicio = validadeInicio,
                ValidadeFim = validadeFim,
                CnpjOuCpfTitular = cnpjCpf,
                RazaoSocialTitular = razaoSocial,
                Thumbprint = thumbprint,
                Emissor = emissor,
                Ativo = true
            };
            _context.CertificadosDigitais.Add(existente);
        }
        else
        {
            existente.NomeArquivo = dto.NomeArquivo;
            existente.ArquivoBytes = arquivoCriptografado;
            existente.SenhaCriptografada = senhaCriptografada;
            existente.ValidadeInicio = validadeInicio;
            existente.ValidadeFim = validadeFim;
            existente.CnpjOuCpfTitular = cnpjCpf;
            existente.RazaoSocialTitular = razaoSocial;
            existente.Thumbprint = thumbprint;
            existente.Emissor = emissor;
            existente.Ativo = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CertificadoDigitalDto
        {
            Id = existente.Id,
            EmpresaId = existente.EmpresaId,
            NomeArquivo = existente.NomeArquivo,
            CnpjOuCpfTitular = existente.CnpjOuCpfTitular,
            RazaoSocialTitular = existente.RazaoSocialTitular,
            Thumbprint = existente.Thumbprint,
            Emissor = existente.Emissor,
            ValidadeInicio = existente.ValidadeInicio,
            ValidadeFim = existente.ValidadeFim,
            EstaExpirado = existente.EstaExpirado,
            DiasParaExpirar = existente.DiasParaExpirar
        };
    }

    public async Task<CertificadoDigitalDto?> ObterCertificadoAtivoAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        var cert = await _context.CertificadosDigitais
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Ativo, cancellationToken);

        if (cert == null) return null;

        return new CertificadoDigitalDto
        {
            Id = cert.Id,
            EmpresaId = cert.EmpresaId,
            NomeArquivo = cert.NomeArquivo,
            CnpjOuCpfTitular = cert.CnpjOuCpfTitular,
            RazaoSocialTitular = cert.RazaoSocialTitular,
            Thumbprint = cert.Thumbprint,
            Emissor = cert.Emissor,
            ValidadeInicio = cert.ValidadeInicio,
            ValidadeFim = cert.ValidadeFim,
            EstaExpirado = cert.EstaExpirado,
            DiasParaExpirar = cert.DiasParaExpirar
        };
    }

    public async Task<X509Certificate2?> CarregarCertificadoX509Async(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        var certDb = await _context.CertificadosDigitais
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Ativo, cancellationToken);

        if (certDb == null || certDb.ArquivoBytes == null || certDb.ArquivoBytes.Length == 0)
            return null;

        byte[] pfxBytes = DescriptografarBytes(certDb.ArquivoBytes);
        string senha = DescriptografarTexto(certDb.SenhaCriptografada);

        return new X509Certificate2(pfxBytes, senha, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
    }

    public async Task RemoverCertificadoAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        var cert = await _context.CertificadosDigitais
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId, cancellationToken);

        if (cert != null)
        {
            _context.CertificadosDigitais.Remove(cert);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #region Métodos de Criptografia AES-256

    private byte[] CriptografarBytes(byte[] dados)
    {
        using var aes = Aes.Create();
        aes.Key = _aesKey;
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length); // Prefixa IV de 16 bytes

        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(dados, 0, dados.Length);
            cs.FlushFinalBlock();
        }

        return ms.ToArray();
    }

    private byte[] DescriptografarBytes(byte[] dadosCriptografados)
    {
        using var aes = Aes.Create();
        aes.Key = _aesKey;

        byte[] iv = new byte[aes.BlockSize / 8];
        Array.Copy(dadosCriptografados, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(
            new MemoryStream(dadosCriptografados, iv.Length, dadosCriptografados.Length - iv.Length),
            aes.CreateDecryptor(),
            CryptoStreamMode.Read))
        {
            cs.CopyTo(ms);
        }

        return ms.ToArray();
    }

    private string CriptografarTexto(string texto)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(texto);
        byte[] criptografado = CriptografarBytes(bytes);
        return Convert.ToBase64String(criptografado);
    }

    private string DescriptografarTexto(string textoBase64)
    {
        byte[] bytes = Convert.FromBase64String(textoBase64);
        byte[] descriptografado = DescriptografarBytes(bytes);
        return Encoding.UTF8.GetString(descriptografado);
    }

    #endregion

    #region Helpers de Extração ICP-Brasil

    private static string ExtrairCnpjOuCpf(string subject)
    {
        // ICP-Brasil frequentemente embute :00000000000000 no CN
        var matchCnpj = Regex.Match(subject, @"\b\d{14}\b|:\s*(\d{14})");
        if (matchCnpj.Success)
            return matchCnpj.Groups[1].Success ? matchCnpj.Groups[1].Value : matchCnpj.Value;

        var matchCpf = Regex.Match(subject, @"\b\d{11}\b|:\s*(\d{11})");
        if (matchCpf.Success)
            return matchCpf.Groups[1].Success ? matchCpf.Groups[1].Value : matchCpf.Value;

        return string.Empty;
    }

    private static string ExtrairNomeTitular(string subject)
    {
        var matchCn = Regex.Match(subject, @"CN=([^,]+)");
        if (matchCn.Success)
        {
            string cn = matchCn.Groups[1].Value.Trim();
            // Remove sufixo com CNPJ/CPF se existir, ex: "EMPRESA XYZ LTDA:12345678000199"
            int indexSeparador = cn.IndexOf(':');
            return indexSeparador > 0 ? cn[..indexSeparador].Trim() : cn;
        }

        return subject;
    }

    #endregion
}
