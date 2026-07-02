import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { tap , switchMap} from 'rxjs/operators';
import { Conversation } from '../models/conversation';
import { ApiService } from '../services/api.service';

@Injectable({
    providedIn:'root'
})
export class ConversationStore{

    private conversationsSubject =    new BehaviorSubject<Conversation[]>([]);
    conversations$ =  this.conversationsSubject.asObservable();
    constructor(private api:ApiService){ }

    load() {

        return this.api
            .getConversations()
            .pipe(
                tap(list => this.conversationsSubject.next(list))
            );
    }

    get value(){
        return this.conversationsSubject.value;
    }
    
    async create(): Promise<string> {

        const id = await firstValueFrom(
            this.api.createConversation()
        );
    
        await firstValueFrom(
            this.load()
        );
    
        return id;
    }


    deleteAndNavigate(id: string, currentId: string): Promise<string | null> {
    
        return firstValueFrom(
            this.delete(id)
        ).then(() => {
    
            const list = this.value;
    
            if (!list.length)
                return null;
    
            if (currentId !== id)
                return currentId;
    
            return list[0].id;
        });
    
    }
   
    async rename(id:string,title:string){

        await firstValueFrom(
            this.api.renameConversation(id,title)
        );
    
        const list=this.value.map(x=>
    
            x.id===id
                ? {...x,title}
                :x
        );
    
        this.conversationsSubject.next(list);
    
    }

    delete(id: string) {

        return this.api
            .deleteConversation(id)
            .pipe(
    
                switchMap(() => this.load())
    
            );
    
    }
}