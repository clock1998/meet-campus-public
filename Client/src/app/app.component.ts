import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { environment } from '../environment';
import { routes } from './app.routes';
import { BreakpointObserver, Breakpoints, MediaMatcher } from '@angular/cdk/layout';
import { Observable, Subject, map, shareReplay, takeUntil } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { NavbarComponent } from "./core/navbar/navbar.component";
@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, NavbarComponent]
})
export class AppComponent {
  destroyed = new Subject<void>();
  text = 'Not Success';
  title = 'client';
  routes = routes;
  isSmallScreen=false;

  private breakpointObserver = inject(BreakpointObserver);
    
  constructor(private http: HttpClient) {
    this.getForecasts();  
  }

  getForecasts() {
    this.http.get<any>(`${environment.apiUrl}/public`).subscribe({
      next: n=>{
        this.text = n.text;
      },
      error: e=>{
        console.error(e);
      }
    }
    );
  }
}
