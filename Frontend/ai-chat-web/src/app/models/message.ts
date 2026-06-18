export interface Message {
    id?: string
    role: string
    content: string
    createdAt: string | Date
  }
  

  export interface ConversationDetailsDto {
    id: string
    title: string
    Messages:Message[]
  }
  