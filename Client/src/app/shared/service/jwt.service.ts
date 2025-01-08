import { inject, Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
interface DecodedToken {
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class JWTService {
  private decodedToken: DecodedToken | null = null;
  private decodeToken(token: string): DecodedToken | null {
    try {
      return jwtDecode(token);
    } catch (error) {
      console.error('Failed to decode token:', error);
      return null;
    }
  }

  getDecodedToken(): DecodedToken | null {
    const appContext = localStorage.getItem('AppContext');
    if (appContext) {
      this.decodedToken = this.decodeToken(JSON.parse(appContext).token);
    }
    return this.decodedToken;
  }
  getExpiryTime(): number | null {
    const token = this.getDecodedToken();
    return token ? token['exp'] : null;
  }

  isTokenExpired(): boolean {
    const exp = this.getExpiryTime();
    if (exp) {
      const expiryTime: number = exp * 1000;
      return expiryTime - Date.now() < 5000;
    }
    return true;
  }
}
