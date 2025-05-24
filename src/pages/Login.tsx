/* eslint-disable react-hooks/exhaustive-deps */

import { ChangeEvent, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { jwtDecode } from 'jwt-decode'
import Cookies from 'js-cookie'


// components
import { Box, Container, Grid } from '@mui/material'
import { BannerImage, FormComponents, Logo, StyledH1, Styledp } from '@/components'

// hooks
import { useFormValidation, usePOST } from '@/hooks'

// utils
import {  jwtExpirationDateConverter, pxToRem } from '@/utils'

// types
import {  MessageProps, LoginData, LoginPostData, DecodedJwt } from '@/types'


// redux
import { useSelector } from 'react-redux'
import { RootState } from '@/redux'

function Login() {
  const  navigate = useNavigate()

  const { email, message } = useSelector((state: RootState) => state.createProfile)

  const inputs = [
    { type: 'email', placeholder: 'Digite seu email' },
    { type: 'password', placeholder: 'Digite sua senha' },
  ]

  const { data, loading, error, postData } = usePOST<LoginData, LoginPostData>('login')
  const { formValues, formValid, handleChange } = useFormValidation(inputs)

  const handleMessage = (): MessageProps => {
    if (!error) return { msg: message ?? '', type: 'success' }

    switch (error) {
      case 401:
        return {
          msg: 'Email e/ou senha inválidos',
          type: 'error',
        }
      default:
        return {
          msg: 'Erro desconhecido, tente novamente mais tarde',
          type: 'error',
        }
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await postData({
      email: String(formValues[0]),
      password: String(formValues[1]),
      name: '',
      phone: ''
    })
  }

  useEffect(() => {
    if (data?.jwt_token) {
      const decoded: DecodedJwt = jwtDecode(data?.jwt_token)
      Cookies.set('Authorization' , data?.jwt_token , {
        expires: jwtExpirationDateConverter(decoded.exp),
    // {  secure: true,} 
        secure: window.location.protocol === 'https:'

      })
      setTimeout(() => {
        navigate('/home')
      }, 100)
      
    }
    if(Cookies.get('Authorization')) navigate('/home');

  }, [data, navigate])

  useEffect(() => {
    if (email) {
      handleChange(0, email)
      }
  }, [email])

  return (
    <Box>
      <Grid container>
        <Grid
          item
          xs={12}
          sm={6}
          sx={{ alignItems: 'center', display: 'flex', height: '100vh' }}
        >
          <Container maxWidth="sm">
            <Box sx={{ marginBottom: pxToRem(24) }}>
              <Logo height={40} width={100} />
            </Box>
            <Box sx={{ marginBottom: pxToRem(24) }}>
              <StyledH1>Bem-Vindo</StyledH1>
              <Styledp>Digite sua senha para logar</Styledp>
            </Box>

            <FormComponents
              inputs={inputs.map((input, index) => ({
                type: input.type,
                placeholder: input.placeholder,
                value: formValues[index] || '',
                onChange: (e: ChangeEvent<HTMLInputElement>) =>
                  handleChange(index, e.target.value),
              }))}
              buttons={[
                {
                  className: 'primary',
                  disabled: !formValid || loading,
                  type: 'submit',
                  onClick: handleSubmit,
                  children: loading ? 'Aguarde ...' : 'Login',
                },
              ]}
              message={handleMessage()}
            />
          </Container>
        </Grid>

        {/* CORRIGIDO: Removido espaço extra em 'none' */}
        <Grid item sm={6} sx={{ display: { xs: 'none', sm: 'block' } }}>
          <BannerImage />
        </Grid>
      </Grid>
    </Box>
  )
}

export default Login
