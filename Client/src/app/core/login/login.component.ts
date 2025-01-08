import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, inject, Output } from '@angular/core';
import { AuthService } from '../../shared/service/auth.service';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { InputErrorDirective } from '../../shared/directive/input-error.directive';
@Component({
    selector: 'app-login-form',
    imports: [ReactiveFormsModule, CommonModule, InputErrorDirective],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginFormComponent {
  @Output() close: EventEmitter<any> = new EventEmitter();
  isLoginDialog = true;
  serverSideErrorMessage = '';
  private authService = inject(AuthService);
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
  });

  signupForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email, emailDomainValidator(/(mcgill.ca)/i)]),
    password: new FormControl('', [Validators.required]),
    passwordConfirm: new FormControl('', [Validators.required]),
  }, {validators: passwordValidator});

  loginFormSubmit() {
    if(this.loginForm.valid){
      this.authService.login(this.loginForm.value.email ?? '', this.loginForm.value.password ?? '').subscribe({
        next: (n)=>{
          localStorage.setItem("AppContext", JSON.stringify(n));
          this.authService.setAppContext(n);
          this.loginForm.reset();
          this.close.emit();
        },
        error: (e:HttpErrorResponse)=>{
          //I want to set errors with error message
          // this.loginForm.setErrors({email: true, password: true});
          this.serverSideErrorMessage = e.error.detail;
          console.log(this.serverSideErrorMessage )
        }
      });
    }
    else{

    } 
  }

  signupFormSubmit() {
    if(this.signupForm.valid){
      this.authService.signup(this.signupForm.value.email ?? '', this.signupForm.value.password ?? '', this.signupForm.value.passwordConfirm ?? '').subscribe({
        next: n=>{
          this.isLoginDialog = true;
          // this.localStorageService.set("AppContext", JSON.stringify(n))
        },
        error: (e:HttpErrorResponse)=>{          
          console.log(e)
          this.serverSideErrorMessage = e.error.detail;
        }
      });
    }
    else{
      
    } 
  }
  toggleLoginDialog() {
    this.isLoginDialog = !this.isLoginDialog;
    this.reset();
  }
  reset() {
    this.serverSideErrorMessage = '';
    this.loginForm.reset();
    this.signupForm.reset();
  }
}
export const passwordValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const password = control.get('password');
  const passwordConfirm = control.get('passwordConfirm');
  return password && passwordConfirm && password.value == passwordConfirm.value ? null : {passwordDoesNotMatch: true} ;
};

export function emailDomainValidator(nameRe: RegExp): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const result = nameRe.test(control.value);
    return result ? null : {emailDomain: {value: control.value}} ;
  };
}