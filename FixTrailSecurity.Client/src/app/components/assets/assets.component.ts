import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Asset } from '../../models/asset.model';
@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="font-family: sans-serif;">
      <h3 style="color: #333; border-bottom: 2px solid #eee; padding-bottom: 10px;">Assets Inventory</h3>
      <div style="background: #f9f9f9; padding: 15px; border-radius: 5px; margin-bottom: 20px; border: 1px solid #ddd;">
        <h4 style="margin-top: 0;">Register New Asset</h4>
        <div style="display: flex; gap: 10px; align-items: center;">
          <input [(ngModel)]="newAsset.name" placeholder="Asset Name (e.g. Web Server)" style="padding: 8px; flex: 1;" />
          <input [(ngModel)]="newAsset.environment" placeholder="Environment (e.g. Production)" style="padding: 8px; flex: 1;" />
          <input [(ngModel)]="newAsset.owner" placeholder="Owner (e.g. Security Team)" style="padding: 8px; flex: 1;" />
          <button (click)="addAsset()" style="padding: 9px 15px; background: #0056b3; color: white; border: none; border-radius: 3px; cursor: pointer;">Add Asset</button>
        </div>
      </div>
      <table style="width: 100%; border-collapse: collapse; text-align: left;">
        <thead>
          <tr style="background: #eee;">
            <th style="padding: 10px; border: 1px solid #ccc;">Name</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Environment</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Owner</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Created At</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let asset of assets" style="border-bottom: 1px solid #eee;">
            <td style="padding: 10px; border: 1px solid #ccc;">{{ asset.name }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">{{ asset.environment }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">{{ asset.owner }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">{{ asset.createdAt | date:'medium' }}</td>
          </tr>
          <tr *ngIf="assets.length === 0">
            <td colspan="4" style="padding: 15px; text-align: center; color: #666; border: 1px solid #ccc;">No assets found. Ensure the .NET API is running.</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class AssetsComponent implements OnInit {
  apiService = inject(ApiService);
  assets: Asset[] = [];
  newAsset: Partial<Asset> = {
    name: '',
    environment: '',
    owner: ''
  };
  ngOnInit(): void {
    this.loadAssets();
  }
  loadAssets(): void {
    this.apiService.getAssets().subscribe({
      next: (data) => this.assets = data,
      error: (err) => console.error('Error loading assets', err)
    });
  }
  addAsset(): void {
    if (!this.newAsset.name) {
      alert('Asset Name is required!');
      return;
    }
    this.apiService.createAsset(this.newAsset).subscribe({
      next: (created) => {
        this.assets.push(created);
        this.newAsset = { name: '', environment: '', owner: '' };
      },
      error: (err) => {
        console.error('Error creating asset', err);
        alert('Failed to create asset. Is the backend API running on port 5000?');
      }
    });
  }
}
