export interface leadsPostData {
 name: string
 email: string
 phone: string
// password: string
 

}

export interface leadsData extends leadsPostData {
    id: number
   }