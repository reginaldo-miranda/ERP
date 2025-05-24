/*import { createGlobalStyle } from 'styled-components'
import { Theme } from '../types/theme'

export const GlobalStyle = createGlobalStyle<{ theme: Theme }>`
   body, html {
     background: ${(props) => props.theme.appBackground};
     margin: 0;
     padding: 0;
     font-family: "Inter", sans-serif;
   }
   
   h1, h2, p, ul, li {
     margin: 0;
     padding: 0;
   }

`
//---------------------------

import { createGlobalStyle } from 'styled-components'
import { Theme } from '../types/theme'

export const GlobalStyle = createGlobalStyle<{ theme: Theme }>`
  *, *::before, *::after {
    box-sizing: border-box;
  }

  html, body {
    background: ${({ theme }) => theme.appBackground};
    margin: 0;
    padding: 0;
    font-family: "Inter", sans-serif;
   
  }

  h1, h2, p, ul, li {
    margin: 0;
    padding: 0;
  }
`
*/

import { pxToRem } from '@/utils'
import { createGlobalStyle } from 'styled-components'
import { DefaultTheme } from 'styled-components'

export const GlobalStyle = createGlobalStyle<{ theme?: DefaultTheme }>`
 
  html, body {
    background: ${({ theme }) => theme.appBackground};
    color: ${(props) => props.theme.appColor};
    margin: 0;
    padding: 0;
    font-family: "Inter", sans-serif;
    
  }

  h1, h2, p, ul, li, figure {
    margin: 0;
    padding: 0;
  }
  .mb-1{
  
    margin-bottom: ${pxToRem(16)};
  }

  .mb-2{
  
    margin-bottom: ${pxToRem(32)};
  }
  
  .skeleton-loading{
      animation: skeletonLoading 2s infinite alternate;
  }

   @keyframes skeletonLoading {
      from {
        background-color: ${(props) => props.theme.appSkeletonFrom};
      to {
        background-color: ${(props) => props.theme.appSkeletonTo};
      }
     .skeleton-loading-mh-1 {
         min-height: ${pxToRem(175)};
    }
         
      .skeleton-loading-mh-2 {
         min-height: ${pxToRem(400)};
    }



`
