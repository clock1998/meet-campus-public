import { Component, effect, ElementRef, inject, ViewChild } from '@angular/core';
import { routes } from '../../app.routes';
import { DialogComponent } from "../dialog/dialog.component";
import { LoginFormComponent } from "../login/login.component";
import { AuthService } from '../../shared/service/auth.service';
@Component({
    selector: 'app-navbar',
    imports: [DialogComponent, LoginFormComponent],
    templateUrl: './navbar.component.html',
    styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  authService = inject(AuthService);
  appContext = this.authService.appContext();
  routes = routes;
  isDark:boolean = false;
  @ViewChild('loginDialog',{ static: true }) loginDialog:ElementRef | undefined;
  constructor() {
    if(localStorage.getItem('isDark') == null) {
      this.isDark = false;
    }
    else{
      this.isDark = JSON.parse(localStorage.getItem('isDark') as string);
    }
    effect(() => {
      this.appContext = this.authService.appContext();
    })
  }
  changeTheme(event:Event ) {
    this.isDark = !this.isDark;
    localStorage.setItem('isDark', JSON.stringify((event.target as HTMLInputElement).checked) );
  }
}
