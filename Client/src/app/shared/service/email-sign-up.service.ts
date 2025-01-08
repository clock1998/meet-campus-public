import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment';
import { AppContext } from '../model/AppContext';
import { JWTService } from './jwt.service';
import { inject, Injectable, signal } from '@angular/core';
export interface SignUpEmailDTO{
  record:string,
  cfTurnstileResponse:string
}
@Injectable({
  providedIn: 'root'
})
export class EmailSignUpService {
  private http=inject(HttpClient);

  signup(dto:SignUpEmailDTO) {
    return this.http.post(`${environment.apiUrl}/SignUpEmail`, dto);
  }

  getCount() {
    return this.http.get<number>(`${environment.apiUrl}/SignUpEmail/GetCount`);
  }
}
