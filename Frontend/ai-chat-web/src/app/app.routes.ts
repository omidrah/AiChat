import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat';
import { LoginComponent } from './pages/login/login';

export const routes: Routes = [
   { path: 'chat/:id', component: ChatComponent },
    {path: 'login', component: LoginComponent}

];
