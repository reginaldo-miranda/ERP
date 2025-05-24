/**
 * convert  text from highlight api
 * @param value - text to be converted
 * @returns  converted text
 */

export function highlightTextConvert(text: string): string {
  switch (text) {
    case 'alert':
       return '* Meta longe de ser batida'
    case 'success':
        return '* A meta do mes foi batida! Parabens!'
    case 'warning':
        return '* Falta pouco, vamos la!'
    default:
        return '* Sem dados no momento'
  }
}