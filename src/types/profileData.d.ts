export interface ProfileEditData{

    name: string
    phone: string
} 

export interface ProfileData extends ProfileEditData{
    email: string
}

export interface CreateProfileData{
    name: string
    email: string
    phone: string
    password: string
    message?: string | null
}