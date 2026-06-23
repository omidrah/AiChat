  import { Component } from '@angular/core';
  import { AuthService } from '../../services/AuthService';
  import { Router } from '@angular/router';
  import { FormsModule } from '@angular/forms';
  import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { firstValueFrom } from 'rxjs';
  @Component({
    selector: 'app-login',
    templateUrl: './login.html',
    standalone: true,
    imports: [FormsModule, CommonModule],
  })
  export class LoginComponent {

    userName = '';
    password = '';
    error = '';

    constructor(
      private auth: AuthService,
      private api: ApiService,
      private router: Router
    ) {}

   async login() {
      this.error = '';

      try {
        await firstValueFrom(this.auth.login(this.userName, this.password));

        const conversationId = await firstValueFrom(this.api.createConversation());

        this.router.navigate(['/chat', conversationId]);
      } catch {
        this.error = 'Login failed';
      }
    }
    
  }
