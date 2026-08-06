import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat';
import { LoginComponent } from './pages/login/login';
import { UserManagement } from './pages/user-management/user-management';
import { OllamaStatus } from './ollama-status/ollama-status';
import { ActiveDirectorySettings } from './pages/active-directory-settings/active-directory-settings';

export const routes: Routes = [
   { path: 'chat/:id', component: ChatComponent }
   ,
   {
      path: 'active-directory',
      component : ActiveDirectorySettings
   }
   ,
   {
      path: 'users',
      component: UserManagement
   },
   { path: 'ollama', component: OllamaStatus },
   { path: 'login', component: LoginComponent },
   { path: '', redirectTo: 'login', pathMatch: 'full' },
   { path: '**', redirectTo: 'login' }
];
