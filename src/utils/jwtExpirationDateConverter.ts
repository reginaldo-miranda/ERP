/**
 * convert JWT exp in day
 * @param exp  - number numer to be convert 
 * @returns the converted exp in day
 */

export function jwtExpirationDateConverter(exp: number): number {
    const currentTime = Math.floor(Date.now()/1000)
    const secondsUntilExpiration = exp - currentTime
    const secondsInADay = 60 * 60 * 24
    const daysUntilExpiration = secondsUntilExpiration / secondsInADay
    return daysUntilExpiration;    
}