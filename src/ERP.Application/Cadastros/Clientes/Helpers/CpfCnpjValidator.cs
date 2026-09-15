namespace ERP.Application.Cadastros.Clientes.Helpers;

/// <summary>
/// Validador local de CPF e CNPJ (algoritmo de dígito verificador).
/// </summary>
public static class CpfCnpjValidator
{
    public static bool ValidarCpf(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11 || cpf.All(c => c == cpf[0])) return false;

        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = 0;
        for (var i = 0; i < 9; i++) soma += int.Parse(cpf[i].ToString()) * mult1[i];
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (d1 != int.Parse(cpf[9].ToString())) return false;

        soma = 0;
        for (var i = 0; i < 10; i++) soma += int.Parse(cpf[i].ToString()) * mult2[i];
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return d2 == int.Parse(cpf[10].ToString());
    }

    public static bool ValidarCnpj(string cnpj)
    {
        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0])) return false;

        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = 0;
        for (var i = 0; i < 12; i++) soma += int.Parse(cnpj[i].ToString()) * mult1[i];
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (d1 != int.Parse(cnpj[12].ToString())) return false;

        soma = 0;
        for (var i = 0; i < 13; i++) soma += int.Parse(cnpj[i].ToString()) * mult2[i];
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return d2 == int.Parse(cnpj[13].ToString());
    }
}
