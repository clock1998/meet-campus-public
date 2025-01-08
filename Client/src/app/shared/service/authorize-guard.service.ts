import { inject, Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, MaybeAsync, GuardResult } from '@angular/router';
import { AuthService } from './auth.service';
import { AppContext } from '../model/AppContext';

@Injectable({
    providedIn: 'root'
})
export class AuthorizeGuard implements CanActivate {
    private authService = inject(AuthService);
    private router = inject(Router);
    
    canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot): MaybeAsync<GuardResult>  {
        if (this.authService.appContext()!=null) {
            if (!this.authService.isAuthenticated()) {
                this.authService.refresh().subscribe({
                    next: (n: AppContext)=>{
                      localStorage.setItem("AppContext", JSON.stringify(n))
                      this.authService.setAppContext(n);
                      //I want to proceed to next route
                      this.router.navigate([next.routeConfig?.path ?? '']);
                    },
                    error: (e: any)=>{
                      this.router.navigate(['']);
                      this.authService.logout();
                      window.location.reload();
                      console.error(e);
                      return false;
                    }
                  })
                // Should Redirect Sig-In Page
            } else {
                return true;
            }
        } else {
            return new Promise((resolve) => {
                this.router.navigate(['/']);
            });
        }
        return false;
    }
}