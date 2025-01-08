import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment';
import { AppContext } from '../model/AppContext';
import { JWTService } from './jwt.service';
import { inject, Injectable, signal } from '@angular/core';
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http=inject(HttpClient);
  private jwtService = inject(JWTService);
  private appContextSignal = signal<AppContext|null>(JSON.parse(localStorage.getItem("AppContext") ?? 'null'));
  readonly appContext = this.appContextSignal.asReadonly();
  login(username: string, password: string) {
    return this.http.post<AppContext>(`${environment.apiUrl}/Auth/Login`, {username, password});
  }

  signup(email: string, password: string, passwordConfirm: string) {
    let firstName = email.split('.')[0];
    let lastName = email.split('.')[1];
    return this.http.post<AppContext>(`${environment.apiUrl}/Auth/Register`, {email, password, passwordConfirm, firstName, lastName});
  }

  logout() {
    localStorage.removeItem("AppContext");
    this.appContextSignal.set(null);
  }

  refresh(){
    return this.http.post<AppContext>(`${environment.apiUrl}/Auth/RefreshToken`, {token: this.appContextSignal()!.token, refreshToken: this.appContextSignal()!.refreshToken});
  }

  isAuthenticated(): boolean {
    return !this.jwtService.isTokenExpired();
  }

  setAppContext(appContext: AppContext) {
    this.appContextSignal.set(appContext);
  }
}
