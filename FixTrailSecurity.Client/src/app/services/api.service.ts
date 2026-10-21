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
  // Default URL when running local dotnet run (HTTPS might use 5001, HTTP uses 5000)
  private apiUrl = 'http://localhost:5000/api';
  // --- Assets ---
  getAssets(): Observable<Asset[]> {
    return this.http.get<Asset[]>(${this.apiUrl}/assets);
  }
  createAsset(asset: Partial<Asset>): Observable<Asset> {
    return this.http.post<Asset>(${this.apiUrl}/assets, asset);
  }
  // --- Findings ---
  getFinding(id: string): Observable<Finding> {
    return this.http.get<Finding>(${this.apiUrl}/findings/);
  }
  createFinding(finding: Partial<Finding>): Observable<any> {
    return this.http.post<any>(${this.apiUrl}/findings, finding);
  }
}
