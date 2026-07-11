import { Component } from '@angular/core';
import { AuthService } from '../../services/AuthService';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { ConversationStore } from '../../store/conversation.store';
@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  standalone: true,
  imports: [FormsModule, CommonModule],
})
export class LoginComponent {

  userName = '';
  password = '';
  error = '';
  mode ='local';

  constructor(
    private auth: AuthService,
    private router: Router,
    private store: ConversationStore
  ) { }

  async ngOnInit() {

    const mode = await firstValueFrom(this.auth.getMode());
    
    this.auth.setMode(this.mode);

    if(mode.mode === 'Windows'){

        await this.windowsLogin();

    }

}

async windowsLogin() {

    await firstValueFrom(
        this.auth.windowsLogin()
    );

    await firstValueFrom(
        this.store.load()
    );

    const list = this.store.value;

    if(list.length){

        this.router.navigate([
            '/chat',
            list[0].id
        ]);

    }else{

        const id =
            await this.store.create();

        this.router.navigate([
            '/chat',
            id
        ]);

    }

}

async login() {
    this.error = '';

    await firstValueFrom(this.auth.login(this.userName, this.password));
    await firstValueFrom(this.store.load());

    const list = this.store.value;

    if (list.length) {
      
      this.router.navigate(['/chat', list[0].id]);

    }
    else {

      const id = await this.store.create();
      this.router.navigate(['/chat', id]);

    }

  }

}
