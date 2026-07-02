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

    rename(id: string, title: string) {

        return this.api
            .renameConversation(id, title)
            .pipe(
    
                switchMap(() => this.load())
    
            );
    
    }

    delete(id: string) {

        return this.api
            .deleteConversation(id)
            .pipe(
    
                switchMap(() => this.load())
    
            );
    
    }
}