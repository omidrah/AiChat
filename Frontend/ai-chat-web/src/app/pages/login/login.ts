import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
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
export class LoginComponent implements OnInit {

    userName = '';
    password = '';
    error = '';
    isSubmitting = false; // فیلد جدید برای مدیریت لودینگ در زمان ارسال فرم
    mode: 'form' | 'windows' = 'form';

    constructor(
        private auth: AuthService,
        private router: Router,
        private store: ConversationStore,
        private cd: ChangeDetectorRef
    ) { }

    async ngOnInit() {
        try {
            const result = await firstValueFrom(this.auth.getMode());
            const backendMode = (result.mode || '').toLowerCase();

            this.mode = (backendMode === 'windowsintegrated' || backendMode === 'windows')
                ? 'windows'
                : 'form';

            this.auth.setMode(this.mode);

            if (this.mode === 'windows') {
                await this.windowsLogin();
            }
        }
        catch (err) {
            this.error = 'دریافت وضعیت احراز هویت انجام نشد. ارتباط با سرور برقرار نیست.';
            this.cd.detectChanges();
            console.error(err);
        }
    }

    async windowsLogin() {
        try {
            this.isSubmitting = true;
            this.cd.detectChanges();
            
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
            this.error = 'ورود خودکار ویندوزی انجام نشد.';
            this.cd.detectChanges();
            console.error(err);
        }
        finally {
            this.isSubmitting = false;
            this.cd.detectChanges();
        }
    }

    async login() {
        if (!this.userName || !this.password || this.isSubmitting) return;

        this.error = '';
        this.isSubmitting = true;
        this.cd.detectChanges();

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
        catch (err: any) {
            if (err.status === 0) {
                this.error = 'ارتباط با سرور برقرار نشد. لطفاً وضعیت شبکه را بررسی کنید.';
            } else {
                this.error = 'نام کاربری یا رمز عبور نادرست است.';
            }
            this.cd.detectChanges();
            console.error(err);
        }
        finally {
            this.isSubmitting = false;
            this.cd.detectChanges();
        }
    }
}
