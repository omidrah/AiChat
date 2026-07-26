import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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

  showForm = false;
  editingUserId: string | null = null;

  form = {
    username: '',
    displayName: '',
    role: 'User',
    password: ''
  };

  constructor(private api: ApiService) {}

  async ngOnInit() {
    await this.loadUsers();
  }

  async loadUsers() {
    try {
      this.users = await firstValueFrom(this.api.getUsers());
    } catch (err: any) {
      this.error = err?.error?.message || 'خطا در دریافت کاربران';
    }
  }

  filteredUsers() {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.users;

    return this.users.filter(x =>
      (x.username || '').toLowerCase().includes(term) ||
      (x.displayName || '').toLowerCase().includes(term) ||
      (x.role || '').toLowerCase().includes(term)
    );
  }

  startCreate() {
    this.showForm = true;
    this.editingUserId = null;
    this.error = '';
    this.form = {
      username: '',
      displayName: '',
      role: 'User',
      password: ''
    };
  }

  editUser(user: any) {
    this.showForm = true;
    this.editingUserId = user.id;
    this.error = '';

    this.form = {
      username: user.username || '',
      displayName: user.displayName || '',
      role: user.role || 'User',
      password: ''
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

      if (this.editingUserId) {
        await firstValueFrom(this.api.updateUser(this.editingUserId, this.form));
      } else {
        await firstValueFrom(this.api.createUser(this.form));
      }

      this.cancelForm();
      await this.loadUsers();
    } catch (err: any) {
      this.error = err?.error?.message || err?.error?.title || 'خطا در ذخیره اطلاعات';
    }
  }

  async deleteUser(id: string) {
    const ok = confirm('آیا از حذف این کاربر مطمئن هستید؟');
    if (!ok) return;

    try {
      await firstValueFrom(this.api.deleteUser(id));
      await this.loadUsers();
    } catch (err: any) {
      this.error = err?.error?.message || 'خطا در حذف کاربر';
    }
  }
}
