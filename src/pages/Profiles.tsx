/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import { ChangeEvent, useContext, useEffect, useState } from "react";
import { CardComponents, FormComponents, Header, StyledH2, StyledButton } from "@/components"
import { AppThemeContext } from "@/context/AppThemeContext";

// MUI

import { Container, Grid } from "@mui/material";

//hook
import { useFormValidation, useGet, usePut,  useDelete } from "@/hooks";

// services
import { logout } from "@/services"


// types
import { InputProps, ProfileData, ProfileEditData, MessageProps } from "@/types";
import Cookies from "js-cookie";

function Profiles() {

  const themeContext = useContext(AppThemeContext)

  // hook

  const [updateMessage, setUpadateMessage] = useState<MessageProps>({
    type: 'success',
    msg: ''
  })
  const clearMessage = () => {
    setTimeout(() => {
      setUpadateMessage({
        type: 'success',
        msg: '',
      })
    }, 3000)
  }
 


  const { data: profileData,
    loading: profileLoading,
    error: profileError, } = useGet<ProfileData>('profile')

  const { 
    data: profileUpdateData,
    putData: profilePutData,
    loading: profileUpdateLoading,
    error: profileUpdateError, } = usePut<ProfileEditData>('profile/update')

   const {
    deleteData: profileDeleteData,
    loading: profileDeleteLoading,

   } = useDelete('profile/update') 

  useEffect(() => {
    if (profileData) {
      handleChange(0, profileData.name)
      handleChange(1, profileData.email)
      handleChange(2, profileData.phone)

    }
  }, [profileData])


  // form
  const inputs: InputProps[] = [
    { name: 'name', type: 'text', placeholder: 'Nome', required: true },
    { name: 'email', type: 'email', placeholder: 'Email', disabled: true },
    { name: 'phone', type: 'tel', placeholder: 'Telefone', required: true },
  ]
  const { formValues, formValid, handleChange } = useFormValidation(inputs)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await profilePutData(
      {
        name: String(formValues[0]),
        phone: String(formValues[2])
      }
    )
  }

  const handleDelete = async () => {
   if (confirm('Tem certeza que deseja excluir sua conta , tem que deletar os leads antes..!')) {
     try {
       await profileDeleteData()
       alert('Deletado com sucesso..!')
       Cookies.remove('Authorization')
       window.location.href = '/' 
     } catch (e) {
        alert('Nao foi possivel fazer a operacao')      
     }
   }
  }

  useEffect(() => {
    if(profileUpdateData !== null){
      setUpadateMessage({
        msg: 'Perfil atualizado com sucesso',
        type: 'success'
      })

    } else if (profileUpdateError){
      setUpadateMessage({
        msg: 'Nao foi possivel realizar a opercao. Entre em contato com nosso suporte',
        type: 'error'
      })
      
    }
    clearMessage()
  
  }, [profileUpdateData, profileUpdateError])

  return (
    <>
      <Header />
      <Container className="mb-2" maxWidth="lg">
        <Grid container spacing={4}>
          <Grid item xs={12} sm={6} >
            {
              !profileError && (
                <CardComponents className={profileLoading ? 'skeleton-loading skeleton-loading-mh-2' : ''} >
                  {
                    !profileLoading && profileData && (
                      <>
                        <StyledH2 className="mb-1">Seus Dados</StyledH2>
                        <FormComponents

                          inputs={inputs.map((input, index) => ({
                            ...input,
                            type: input.type,
                            placeholder: input.placeholder,
                            value: formValues[index] || '',
                            onChange: (e: ChangeEvent<HTMLInputElement>) =>
                              handleChange(index, (e.target as HTMLInputElement).value),
                          }))}

                          buttons={[
                            {
                              className: 'primary',
                              disabled: !formValid || profileUpdateLoading,
                              type: 'submit',
                              onClick: handleSubmit,
                              children: profileUpdateLoading ? 'Aguarde..': 'Atualizar meu perfil',
                            },
                            {
                              className: 'alert',
                              disabled: profileDeleteLoading,
                              type: 'button',
                              onClick: handleDelete,
                              children: profileDeleteLoading ? 'Aguarde...':'Excluir minha conta',
                            }
                          ]}
                          message={updateMessage}
                        />
                      </>
                    )
                  }
                </CardComponents>
              )
            }
          </Grid>
          <Grid item xs={12} sm={6} >
            <CardComponents>
              <StyledH2 className="mb-1">Definiçoes de Conta</StyledH2>
              <StyledButton className="primary mb-1" onClick={themeContext?.toggleTheme}  >
                Trocar para tema{' '}
                {themeContext?.appTheme === 'light' ? 'escuro' : 'claro'}
              </StyledButton>
              <StyledButton className='alert' onClick={logout} >
                Logout
              </StyledButton>

            </CardComponents>
          </Grid>
        </Grid>
        {/* Add your profile cards here */}
      </Container>
      
    </>
  )
}

export default Profiles;