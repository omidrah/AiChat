import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagement implements OnInit {

  users: any[] = [];
  searchTerm = '';
  error = '';
  isLoading = false;

  showForm = false;
  editingUserId: string | null = null;

  // تعریف ساختار فرم با فیلدهای جدید
  form = {
    userName: '',
    displayName: '',
    role: 'User',
    password: '',
    isActive: true, // مقدار پیش‌فرض فعال
    authProvider: 'Local' // مقدار پیش‌فرض سیستم محلی
  };

  constructor(private api: ApiService, private cd: ChangeDetectorRef) { }

  async ngOnInit() {
    await this.loadUsers();
  }

  async loadUsers() {
    try {
      this.isLoading = true;
      this.error = '';
      this.cd.detectChanges();

      const response = await firstValueFrom(this.api.getUsers());
      
      this.users = (response || [])
        .map(x => ({
          ...x,
          userName: x.userName || x.username || '',
          displayName: x.displayName || '',
          role: x.role || 'User',
          authProvider: x.authProvider || 'Local',
          isActive: x.isActive !== undefined ? x.isActive : true,
          isSystemUser: (x.userName || x.username || '').toLowerCase() === 'administrator' || 
                        (x.userName || x.username || '').toLowerCase() === 'admin'
        }))
        // یک فیلتر پشتیبان در فرانت برای اطمینان از عدم نمایش اکانت‌های ادمین اصلی
        .filter(x => !x.isSystemUser);

    } catch (err: any) {
      console.error('Load Error:', err);
       // شناسایی قطع بودن سرور یا خطای شبکه
        if (err.name === 'HttpErrorResponse' && err.status === 0) {
          this.error = '⚠️ ارتباط با سرور برقرار نشد. لطفاً از اتصال اینترنت یا روشن بودن سرور مطمئن شوید.';
        } else {
          this.error = err?.error?.message || 'خطا در دریافت اطلاعات از سرور؛ مجدداً تلاش کنید.';
        }      
    } finally {
      this.isLoading = false;
      this.cd.detectChanges();
    }
  }

  canEdit(user: any) {
    return !user.isSystemUser;
  }

  canDelete(user: any) {
    return !user.isSystemUser;
  }

  filteredUsers() {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.users;

    return this.users.filter(x =>
      (x.userName || '').toLowerCase().includes(term) ||
      (x.displayName || '').toLowerCase().includes(term) ||
      (x.authProvider || '').toLowerCase().includes(term)
    );
  }

  startCreate() {
    this.showForm = true;
    this.editingUserId = null;
    this.error = '';
    this.form = {
      userName: '',
      displayName: '',
      role: 'User',
      password: '',
      isActive: true,
      authProvider: 'Local'
    };
  }

  editUser(user: any) {
    this.showForm = true;
    this.editingUserId = user.id;
    this.error = '';

    this.form = {
      userName: user.userName || '',
      displayName: user.displayName || '',
      role: user.role || 'User',
      password: '',
      isActive: user.isActive,
      authProvider: user.authProvider || 'Local'
    };
  }

  cancelForm() {
    this.showForm = false;
    this.editingUserId = null;
    this.error = '';
  }

  async saveUser() {
    try {
      this.error = '';

      // ارسال شی فرم که حاوی isActive و authProvider جدید است
      if (this.editingUserId) {
        await firstValueFrom(this.api.updateUser(this.editingUserId, this.form));
      } else {
        await firstValueFrom(this.api.createUser(this.form));
      }

      this.cancelForm();
      await this.loadUsers();
    } catch (err: any) {
      
      // شناسایی قطع بودن سرور یا خطای شبکه
      if (err.name === 'HttpErrorResponse' && err.status === 0) {
        this.error = '⚠️ ارتباط با سرور برقرار نشد. لطفاً از اتصال اینترنت یا روشن بودن سرور مطمئن شوید.';
      } else {
        this.error = err?.error?.message || err?.error?.title || 'خطا در ذخیره اطلاعات';
      }     

      this.cd.detectChanges();
    }
  }

  async deleteUser(id: string) {
    const ok = confirm('آیا از حذف این کاربر مطمئن هستید؟');
    if (!ok) return;

    try {
      await firstValueFrom(this.api.deleteUser(id));
      await this.loadUsers();
    } catch (err: any) {

      // شناسایی قطع بودن سرور یا خطای شبکه
        if (err.name === 'HttpErrorResponse' && err.status === 0) {
          this.error = '⚠️ ارتباط با سرور برقرار نشد. لطفاً از اتصال اینترنت یا روشن بودن سرور مطمئن شوید.';
        } else {
          this.error = err?.error?.message || 'خطا در حذف کاربر';
        }    
      
        this.cd.detectChanges();
    }
  }
}
