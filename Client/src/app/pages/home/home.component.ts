import { Component, ElementRef, inject, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { NgxTurnstileModule, NgxTurnstileFormsModule} from "ngx-turnstile";
import {MatTabsModule} from '@angular/material/tabs';
import { EmailSignUpService, SignUpEmailDTO } from '../../shared/service/email-sign-up.service';
import { DialogComponent } from "../../core/dialog/dialog.component";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { InputErrorDirective } from '../../shared/directive/input-error.directive';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-home',
    imports: [MatTabsModule, DialogComponent, ReactiveFormsModule, CommonModule, InputErrorDirective, NgxTurnstileModule, NgxTurnstileFormsModule],
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss'
})
export class HomeComponent {
  @ViewChildren ('animatedText') animatedTexts!: QueryList<ElementRef>;
  @ViewChild ('dialog') dialog!: DialogComponent;
  emailSignUpService = inject(EmailSignUpService);
  submitButtonDisabled = true;
  serverSideErrorMessage: string = '';
  emailSignUpForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    turnstile: new FormControl(''),
  })
  observer:IntersectionObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('show');
      } else {
        entry.target.classList.remove('show');
      }
    });
  });

  ngAfterViewInit() {
    this.animatedTexts.forEach((el) => {
      this.observer.observe(el.nativeElement);
    });
  }

  sendCaptchaResponse(captchaResponse: string|null) {
    this.submitButtonDisabled = false;
    // console.log(`Resolved captcha with response: ${captchaResponse}`);
  }

  emailSignUpFormSubmit() {
    const dto:SignUpEmailDTO = {
      record:this.emailSignUpForm.value.email!,
      cfTurnstileResponse:this.emailSignUpForm.value.turnstile!,
    } 
    this.emailSignUpService.signup(dto).subscribe({
      next: n=>{
        this.emailSignUpForm.reset();
        this.dialog.closeDialog();
      },
      error: (e:HttpErrorResponse)=>{          
        this.serverSideErrorMessage = e.error.detail;
        console.log(this.serverSideErrorMessage )
      }
    });
  }
}
