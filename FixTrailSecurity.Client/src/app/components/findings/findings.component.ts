import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { Finding } from '../../models/finding.model';
@Component({
  selector: 'app-findings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div style="font-family: sans-serif;">
      <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #eee; padding-bottom: 10px;">
        <h3 style="color: #333; margin: 0;">Findings for Asset</h3>
        <a routerLink="/assets" style="color: #0056b3; text-decoration: none; font-size: 14px;">&larr; Back to Assets</a>
      </div>
      <div style="display: flex; gap: 20px;">
        <div style="flex: 1; background: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #ffeeba;">
          <h4 style="margin-top: 0; color: #856404;">Report New Finding</h4>
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
            <input [(ngModel)]="newFinding.title" placeholder="Finding Title" style="padding: 8px;" />
            <input [(ngModel)]="newFinding.source" placeholder="Source Scanner" style="padding: 8px;" />
            <input [(ngModel)]="newFinding.sourceSeverity" placeholder="Severity" style="padding: 8px;" />
            <input [(ngModel)]="newFinding.externalId" placeholder="External ID / CVE" style="padding: 8px;" />
          </div>
          <textarea [(ngModel)]="newFinding.description" placeholder="Description details..." style="width: 100%; margin-top: 10px; padding: 8px; box-sizing: border-box;"></textarea>
          <button (click)="addFinding()" style="margin-top: 10px; padding: 9px 15px; background: #d39e00; color: white; border: none; border-radius: 3px; cursor: pointer;">Save Finding</button>
        </div>
        <div style="flex: 1; background: #e2e3e5; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #d6d8db;">
          <h4 style="margin-top: 0; color: #383d41;">Batch Import (.json / .xml)</h4>
          <input type="file" accept=".json,.xml" (change)="onFileSelected($event)" style="margin-bottom: 15px;" />
          <br/>
          <button (click)="uploadFile()" [disabled]="!selectedFile" [ngStyle]="{'background': selectedFile ? '#28a745' : '#ccc'}" style="padding: 9px 15px; color: white; border: none; border-radius: 3px; cursor: pointer;">Upload & Parse</button>
        </div>
      </div>
      <table style="width: 100%; border-collapse: collapse; text-align: left;">
        <thead>
          <tr style="background: #eee;">
            <th style="padding: 10px; border: 1px solid #ccc;">Title</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Severity</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Source</th>
            <th style="padding: 10px; border: 1px solid #ccc;">Status</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let f of findings" style="border-bottom: 1px solid #eee;">
            <td style="padding: 10px; border: 1px solid #ccc;">{{ f.title }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">{{ f.sourceSeverity }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">{{ f.source }}</td>
            <td style="padding: 10px; border: 1px solid #ccc;">
              <span [ngStyle]="{'color': f.status === 0 ? 'red' : 'green'}">
                {{ f.status === 0 ? 'Open' : f.status === 1 ? 'Assigned' : f.status === 2 ? 'Mitigated' : f.status === 3 ? 'Retesting' : f.status === 4 ? 'Closed' : 'Reopened' }}
              </span>
            </td>
          </tr>
          <tr *ngIf="findings.length === 0">
            <td colspan="4" style="padding: 15px; text-align: center; color: #666; border: 1px solid #ccc;">
              No findings recorded for this asset yet.
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class FindingsComponent implements OnInit {
  apiService = inject(ApiService);
  route = inject(ActivatedRoute);
  assetId: string = '';
  selectedFile: File | null = null;
  findings: Finding[] = [];
  newFinding: Partial<Finding> = { title: '', source: '', sourceSeverity: '', description: '', externalId: '' };
  ngOnInit(): void {
    this.assetId = this.route.snapshot.paramMap.get('assetId') || '';
    this.newFinding.assetId = this.assetId;
    this.loadFindings();
  }
  loadFindings(): void {
    this.apiService.getFindingsByAsset(this.assetId).subscribe({
      next: (data) => this.findings = data,
      error: (err) => console.error('Error loading findings', err)
    });
  }
  addFinding(): void {
    if (!this.newFinding.title || !this.newFinding.source || !this.newFinding.sourceSeverity) {
      alert('Title, Source, and Severity are required!');
      return;
    }
    this.apiService.createFinding(this.newFinding).subscribe({
      next: (res) => {
        alert('Finding recorded successfully!');
        this.newFinding = { assetId: this.assetId, title: '', source: '', sourceSeverity: '', description: '', externalId: '' };
        this.loadFindings();
      },
      error: (err) => alert(err.error?.message || 'Failed to create finding.')
    });
  }
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && (file.name.endsWith('.json') || file.name.endsWith('.xml'))) {
      this.selectedFile = file;
    } else {
      this.selectedFile = null;
      alert('Please select a valid .json or .xml file.');
    }
  }
  uploadFile(): void {
    if (!this.selectedFile) return;
    this.apiService.uploadFindingsFile(this.assetId, this.selectedFile).subscribe({
      next: (res) => {
        alert(`${res.message}\nNew: ${res.newFindingsAdded}, Updated: ${res.existingFindingsUpdated}`);
        this.selectedFile = null;
        this.loadFindings();
      },
      error: (err) => {
        alert(err.error?.message || 'Failed to upload file.');
      }
    });
  }
}
