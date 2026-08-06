import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActiveDirectoryAdminApi } from '../../services/active-directory-admin.service';
import { ActiveDirectoryDiagnosticResult, ActiveDirectoryDiagnosticType } from '../../models/ActiveDirectory';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-active-directory-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './active-directory-settings.html',
  styleUrl: './active-directory-settings.css',
})
export class ActiveDirectorySettings implements OnInit {

  private readonly fb = inject(FormBuilder);
  private readonly adApi = inject(ActiveDirectoryAdminApi);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly diagnosticType = ActiveDirectoryDiagnosticType;

  loading = false;
  saving = false;
  testing = false;

  result: ActiveDirectoryDiagnosticResult | null = null;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  readonly form = this.fb.nonNullable.group({
    enabled: true,
    domain: ['', [Validators.required]],
    container: [''],
    server: ['', [Validators.required]],
    useSsl: false,
    servers: this.fb.array([])
  });

  get servers(): FormArray {
    return this.form.controls.servers;
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.loading = true;
    this.errorMessage = null;

    this.adApi.getSettings()
      .subscribe({
        next: settings => {
          console.log('AD Settings received from API:', settings);


          this.form.patchValue({
            enabled: settings.enabled,
            domain: settings.domain,
            container: settings.container,
            server: settings.server,
            useSsl: settings.useSsl
          });

          this.servers.clear();

          const servers = settings.servers?.length
            ? settings.servers
            : (settings.server ? [settings.server] : []);

          for (const server of servers) {
            this.servers.push(this.fb.nonNullable.control(server));
          }

          this.loading = false;
          this.cdr.detectChanges(); // ۳. فورس کردن انگولار به رندر مجدد HTML

        },
        error: error => {
          console.error('Error loading AD settings:', error);
          this.loading = false;
          this.cdr.detectChanges(); // ۴. فورس رندر در حالت خطا
          this.errorMessage =
            error?.error?.message ?? 'دریافت تنظیمات Active Directory ناموفق بود.';
        }
      });
  }

  addServer(value = ''): void {
    this.servers.push(this.fb.nonNullable.control(value));
  }

  removeServer(index: number): void {
    this.servers.removeAt(index);
  }

  save(): void {
    this.successMessage = null;
    this.errorMessage = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;

    const value = this.form.getRawValue();

    this.adApi.updateSettings({
      enabled: value.enabled,
      domain: value.domain.trim(),
      container: value.container.trim(),
      server: value.server.trim(),
      useSsl: value.useSsl,
      servers: (value.servers as string[])
        .map(x => x.trim())
        .filter(Boolean)
    })
      .pipe(finalize(() => this.saving = false))
      .subscribe({
        next: () => {
          this.successMessage =
            'تنظیمات با موفقیت ذخیره شد و در درخواست‌های بعدی اعمال می‌شود.';
        },
        error: error => {
          this.errorMessage =
            error?.error?.message ?? 'ذخیره تنظیمات ناموفق بود.';
        }
      });
  }

  runTest(type: ActiveDirectoryDiagnosticType): void {
    this.testing = true;
    this.result = null;
    this.errorMessage = null;

    const raw = this.form.getRawValue();

    this.adApi.runDiagnostic({
      type,
      server: raw.server.trim() || null,
      domain: raw.domain.trim() || null
    })
      .pipe(finalize(() => this.testing = false))
      .subscribe({
        next: result => this.result = result,
        error: error => {
          this.errorMessage =
            error?.error?.message ?? 'اجرای تست ناموفق بود.';
        }
      });
  }
}
