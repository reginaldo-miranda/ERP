import { HtmlHTMLAttributes } from "react";

export type InputProps = React.HTMLInputTypeAttribute<HTMLInputElement>
export type ButtonProps = React.HTMLButtonElement<HtmlHTMLAttributes>

export type MessageProps = {
    msg: string
    type: 'error' | 'success'; 
}

export interface FormComponentsProps {
    inputs : InputProps[]
    buttons: ButtonProps[]
    message?: MessageProps
}