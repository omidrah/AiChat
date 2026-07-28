export interface Message {
    id?: string
    role: 'user' | 'assistant'; 
    content: string
    createdAt: string | Date
    model?: string
    copied?: boolean
  }
  

  export interface ConversationDetailsDto {
    id: string
    title: string
    Messages:Message[]
  }
  