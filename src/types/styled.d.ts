// Definição de tipos para styled-components
import 'styled-components'
import { Theme } from './theme'

declare module 'styled-components' {
  export interface DefaultTheme extends Theme {
    /**
     * Propriedade de marcação para TypeScript
     * @internal
     */
    __brand?: never
  }
}
