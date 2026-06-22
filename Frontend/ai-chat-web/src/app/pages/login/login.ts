  import { Component } from '@angular/core';
  import { AuthService } from '../../services/AuthService';
  import { Router } from '@angular/router';
  import { FormsModule } from '@angular/forms';
  import { CommonModule } from '@angular/common';
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
      private router: Router
    ) {}

    login() {

      this.error = '';

      this.auth.login(this.userName, this.password)
        .subscribe({
          next: () => {
            this.router.navigate(['/chat']);
          },
          error: () => {
            this.error = 'Login failed';
          }
        });
    }
  }
