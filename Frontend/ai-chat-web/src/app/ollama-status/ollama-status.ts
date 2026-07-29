import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../services/api.service';
import { OllamaServerDetails } from '../models/AiHealthStatus';

@Component({
  selector: 'app-ollama-status',
    imports: [CommonModule],
  standalone: true,

  templateUrl: './ollama-status.html',
  styleUrl: './ollama-status.css',
})
export class OllamaStatus implements OnInit {

  private api = inject(ApiService);
  private snackBar = inject(MatSnackBar);

  loading = signal(true);
  details = signal<OllamaServerDetails | null>(null);

  ngOnInit() {
    this.fetchDetails();
  }

    fetchDetails() {
    this.loading.set(true);
    this.api.getOllamaDetails().subscribe({
      next: (res) => {
        this.details.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.snackBar.open('خطا در برقراری ارتباط با Ollama سرور', 'بستن', { duration: 3000 });
      }
    });
  }

}
