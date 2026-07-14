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
    /**
     * form → برای Local و ActiveDirectory
     * windows → برای WindowsIntegrated  -> هنوز در نظر گرفته نشده است...
     */
    mode: 'form' | 'windows' = 'form';

    constructor(
        private auth: AuthService,
        private router: Router,
        private store: ConversationStore
    ) { }

    async ngOnInit() {

        try {

            const result = await firstValueFrom(this.auth.getMode());
            const backendMode = (result.mode || '').toLowerCase();

            this.mode = backendMode === 'windowsintegrated' || backendMode === 'windows'
                ? 'windows'
                : 'form';

            this.auth.setMode(this.mode);

            if (this.mode === 'windows') {
                await this.windowsLogin();
            }
        }
        catch (err) {
            this.error = 'دریافت وضعیت احراز هویت انجام نشد.';
            console.error(err);
        }
    }

    async windowsLogin() {
        try {
            await firstValueFrom(this.auth.windowsLogin());
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
        catch (err) {
            this.error = 'ورود ویندوزی انجام نشد.';
            console.error(err);
        }

    }

    async login() {

        this.error = '';
        try {
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
        catch (err) {
            this.error = 'نام کاربری یا رمز عبور نادرست است.';
            console.error(err);
        }
    }

}
