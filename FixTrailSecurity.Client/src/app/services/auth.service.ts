import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = 'http://localhost:5000/api/auth';
  private tokenKey = 'jwt_token';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  constructor() {
    const token = sessionStorage.getItem(this.tokenKey);
    if (token) this.currentUserSubject.next({ token });
  }
  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        if (res && res.token) {
          sessionStorage.setItem(this.tokenKey, res.token);
          this.currentUserSubject.next(res);
        }
      })
    );
  }
  logout() {
    sessionStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }
  getToken(): string | null { return sessionStorage.getItem(this.tokenKey); }
  isLoggedIn(): boolean { return !!this.getToken(); }
}
