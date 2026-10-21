import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Asset } from '../models/asset.model';
import { Finding } from '../models/finding.model';
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api';
  getAssets(): Observable<Asset[]> {
    return this.http.get<Asset[]>(`${this.apiUrl}/assets`);
  }
  createAsset(asset: Partial<Asset>): Observable<Asset> {
    return this.http.post<Asset>(`${this.apiUrl}/assets`, asset);
  }
  getFinding(id: string): Observable<Finding> {
    return this.http.get<Finding>(`${this.apiUrl}/findings/${id}`);
  }
  createFinding(finding: Partial<Finding>): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/findings`, finding);
  }
}
