import { Component, ElementRef, inject, QueryList, ViewChildren } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { EmailSignUpService } from '../../shared/service/email-sign-up.service';

@Component({
    selector: 'app-mission',
    imports: [MatTabsModule],
    templateUrl: './mission.component.html',
    styleUrl: './mission.component.scss'
})
export class MissionComponent {
  emailsCount: number = 0;
  @ViewChildren ('animated') animated!: QueryList<ElementRef>;
  emailSignUpService = inject(EmailSignUpService);
  serverSideErrorMessage: string = '';
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
    this.animated.forEach((el) => {
      this.observer.observe(el.nativeElement);
    });
  }
  ngOnInit() {
    this.emailSignUpService.getCount().subscribe({
      next: n => this.emailsCount = n
    });
  }
}
