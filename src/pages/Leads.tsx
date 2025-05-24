/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { ChangeEvent, useState, useEffect } from 'react'

// components
import {
  CardComponents,
  CustomTable,
  FormComponents,
  Header,
  StyledH2,
  StyledButton,
  Styledspan,
  Styledp,
} from '@/components'

// hooks
import { useFormValidation, useGet, usePOST, useDelete } from '@/hooks'

// mui
import { Container, Grid } from '@mui/material'

// types
import { InputProps, leadsData, leadsPostData, MessageProps } from '@/types'

function Leads() {
  // POST criar lead
  const {
    data: createLeadsData,
    loading: createLeadsLoading,
    error: createLeadsError,
    postData: createLeadsPostData,
  } = usePOST<leadsData, leadsPostData>('leads/create', true)

  // GET listar leads
  const {
    data: leadsData,
    loading: leadsLoading,
    error: leadsError,
    getData: getLeads,
  } = useGet<leadsData[]>('leads')

  // DELETE lead
  const { deleteData: leadsDeleteData, loading: leadsDeleteLoading } =
    useDelete('leads/delete')

  // Campos do formulário
  const inputs: InputProps[] = [
    { name: 'name', type: 'text', placeholder: 'Nome', required: true },
    { name: 'email', type: 'email', placeholder: 'Email', required: true },
    { name: 'phone', type: 'tel', placeholder: 'Telefone', required: true },
  ]
  const { formValues, formValid, handleChange } = useFormValidation(inputs)

  const [createMessage, setCreateMessage] = useState<MessageProps>({
    type: 'success',
    msg: '',
  })

  const clearMessage = () => {
    setTimeout(() => {
      setCreateMessage({ type: 'success', msg: '' })
    }, 3000)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    await createLeadsPostData({
      name: String(formValues[0]),
      email: String(formValues[1]),
      phone: String(formValues[2]),
 
    })
  }

  const handleDelete = async (id: number) => {
    if (confirm('Tem certeza que deseja excluir seu lead?')) {
      try {
        await leadsDeleteData({ params: { id: id } })
        alert('Lead deletado com sucesso!')
        getLeads()
      } catch (e) {
        alert('Não foi possível fazer a operação')
      }
    }
  }

  useEffect(() => {
    if (createLeadsData?.id) {
      setCreateMessage({
        msg: 'Lead criado com sucesso!',
        type: 'success',
      })
      //  resetForm()
      getLeads()
      clearMessage()
    } else if (createLeadsError) {
      setCreateMessage({
        msg: 'Não foi possível criar o lead.',
        type: 'error',
      })
      clearMessage()
    } else {
      clearMessage()
    }
  }, [createLeadsData, createLeadsError])

  return (
    <>
      <Header />
      <Container className="mb-2" maxWidth="lg">
        <Grid container spacing={4}>
          <Grid item xs={12} sm={7}>
            <CardComponents
              className={leadsLoading ? 'skeleton-loading skeleton-mh-2' : ''}
            >
              {!leadsError && !leadsLoading && (
                <>
                  <StyledH2 className="mb-1">Meus leads</StyledH2>
                  {leadsData?.length ? (
                    <CustomTable
                      headers={['Nome', 'Email', 'Telefone', '']}
                      rows={leadsData.map((lead) => [
                        <Styledp>{lead.name}</Styledp>,
                        <Styledp>{lead.email}</Styledp>,
                        <Styledp>{lead.phone}</Styledp>,
                        <StyledButton
                          className="borderless-alert"
                          onClick={() => handleDelete(lead.id)}
                          disabled={leadsDeleteLoading}
                        >
                          Excluir
                        </StyledButton>,
                      ])}
                    />
                  ) : (
                    <Styledspan>sem leads cadastrados</Styledspan>
                  )}
                </>
              )}
            </CardComponents>
          </Grid>

          <Grid item xs={12} sm={5}>
            <CardComponents>
              <StyledH2 className="mb-1">Cadastrar leads</StyledH2>
              <FormComponents
                inputs={inputs.map((input, index) => ({
                  ...input,
                  value: formValues[index] || '',
                  onChange: (e: ChangeEvent<HTMLInputElement>) =>
                    handleChange(index, e.target.value),
                }))}
                buttons={[
                  {
                    className: 'primary',
                    disabled:
                      !formValid || createLeadsLoading || leadsDeleteLoading,
                    type: 'submit',
                    onClick: handleSubmit,
                    children: 'Cadastrar lead',
                  },
                ]}
                message={createMessage}
              />
            </CardComponents>
          </Grid>
        </Grid>
      </Container>
    </>
  )
}

export default Leads
