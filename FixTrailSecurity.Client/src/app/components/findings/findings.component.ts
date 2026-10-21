import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { Finding, FindingStatus } from '../../models/finding.model';
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
      <div style="background: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #ffeeba;">
        <h4 style="margin-top: 0; color: #856404;">Report New Finding</h4>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
          <input [(ngModel)]="newFinding.title" placeholder="Finding Title (e.g. SQL Injection)" style="padding: 8px;" />
          <input [(ngModel)]="newFinding.source" placeholder="Source Scanner (e.g. Nmap, ZAP)" style="padding: 8px;" />
          <input [(ngModel)]="newFinding.sourceSeverity" placeholder="Severity (e.g. High, Medium)" style="padding: 8px;" />
          <input [(ngModel)]="newFinding.externalId" placeholder="External ID / CVE (Optional)" style="padding: 8px;" />
        </div>
        <textarea [(ngModel)]="newFinding.description" placeholder="Description details..." style="width: 100%; margin-top: 10px; padding: 8px; box-sizing: border-box;"></textarea>
        <button (click)="addFinding()" style="margin-top: 10px; padding: 9px 15px; background: #d39e00; color: white; border: none; border-radius: 3px; cursor: pointer;">Save Finding</button>
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
          <tr>
            <td colspan="4" style="padding: 15px; text-align: center; color: #666; border: 1px solid #ccc;">
              Finding list specific to Asset ID: {{ assetId }} will appear here. (Requires GET /findings/asset/id API)
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
  findings: Finding[] = [];
  newFinding: Partial<Finding> = {
    title: '',
    source: '',
    sourceSeverity: '',
    description: '',
    externalId: ''
  };
  ngOnInit(): void {
    this.assetId = this.route.snapshot.paramMap.get('assetId') || '';
    this.newFinding.assetId = this.assetId;
  }
  addFinding(): void {
    if (!this.newFinding.title || !this.newFinding.source || !this.newFinding.sourceSeverity) {
      alert('Title, Source, and Severity are required!');
      return;
    }
    this.apiService.createFinding(this.newFinding).subscribe({
      next: (res) => {
        alert(res.message || 'Finding recorded successfully!');
        this.newFinding = { assetId: this.assetId, title: '', source: '', sourceSeverity: '', description: '', externalId: '' };
      },
      error: (err) => {
        console.error('Error creating finding', err);
        alert('Failed to create finding.');
      }
    });
  }
}
