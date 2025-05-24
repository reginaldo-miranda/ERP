/**
 * convert pixels to rem
 * @param pixels - pixels value to be convert
 * @returns the converted rem value
 */

export function pxToRem(pixels: number): string {
    return  `${pixels / 16}rem`;
}