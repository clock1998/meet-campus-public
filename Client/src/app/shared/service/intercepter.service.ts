import { HttpEvent, HttpEventType, HttpHandler, HttpHandlerFn, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable, tap } from "rxjs";
import { AuthService } from "./auth.service";
export function UniversalAppInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    // Inject the current `AuthService` and use it to get an authentication token:
    let authService = inject(AuthService);
    let appContext =authService.appContext();
    if (appContext != null) {
        req = req.clone({
            url: req.url,
            setHeaders: {
                Authorization: `Bearer ${appContext.token}`
            }
        });
        return next(req).pipe(tap((event) => {
            //I want to call refresh token if 401
            if (event.type === HttpEventType.Response) {
                //I want to call refresh token if 401
                if( event.status == 401 && authService.appContext()!=null) {
                    console.log(req.url, '401 Refresh');
                    authService.refresh();
                    location.reload();
                }
            }
        }));
    }
    return next(req);
}