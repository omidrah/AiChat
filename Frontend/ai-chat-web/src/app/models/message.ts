export interface Message {
    id?: string
    role: 'user' | 'assistant'; 
    content: string
    createdAt: string | Date
    copied?: boolean
  }
  

  export interface ConversationDetailsDto {
    id: string
    title: string
    Messages:Message[]
  }
  