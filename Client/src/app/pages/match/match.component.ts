// import { Component, ElementRef, inject, ViewChild } from '@angular/core';
// import { SignalRService } from '../shared/service/signalr.service';
// import { WebRTCService } from '../shared/service/webrtc.service';
// import { Transaction } from '../shared/model/Transaction';
// import { AuthService } from '../shared/service/auth.service';

// @Component({
//   selector: 'app-match',
//   standalone: true,
//   imports: [],
//   templateUrl: './match.component.html',
//   styleUrl: './match.component.scss'
// })
// export class MatchComponent {
//   peerConnection: RTCPeerConnection;

//   localStream!: MediaStream;
//   remoteStream: MediaStream = new MediaStream();
//   signalRService = inject(SignalRService);
//   authService = inject(AuthService);
//   emails: string[] = [];
//   isMyOffer = false;
//   @ViewChild('localVideo', { static: true, read: ElementRef }) localVideo!: ElementRef;
//   @ViewChild('remoteVideo', { static: true, read: ElementRef }) remoteVideo!: ElementRef;
//   @ViewChild('answerBtn', { static: true, read: ElementRef }) answerBtn!: ElementRef;

//   constructor() {
//     this.peerConnection = new RTCPeerConnection({
//       iceServers: [{
//         urls: ['stun:stun.l.google.com:19302', 'stun:stun1.l.google.com:19302', 'stun:stun2.l.google.com:19302', 'stun:stun3.l.google.com:19302', 'stun:stun4.l.google.com:19302', 'turn:freeturn.net:3479'],
//         username: 'free',
//         credential: 'free'
//       }]
//     });
//   }

//   async ngOnInit(): Promise<void> {
//     this.signalRService.startConnection();
//     this.signalRService.addQueueJoinedListener((emails: string[]) => {
//       this.emails = emails;
//     });

//     this.signalRService.newOfferAwaitingListener(async (transaction) => {
//       this.answerBtn.nativeElement.disabled = false;
//       this.answerBtn.nativeElement.addEventListener('click', () => {
//         this.answer(transaction)
//       })
//     });

//     this.signalRService.AnswerResponseListener(async (transaction: Transaction) => {
//       await this.peerConnection.setRemoteDescription(JSON.parse(transaction.answer!))
//     })
    
//     this.signalRService.AnswererResponseListener((transaction: Transaction) => {
//       let offerIceCandidates:RTCIceCandidateInit[] = transaction.offerIceCandidates.map(n => JSON.parse(n));
//       offerIceCandidates.forEach(candidate => {
//         this.peerConnection.addIceCandidate(candidate);
//         console.log("======Added Ice Candidate======")
//       })
//     });

//     this.signalRService.receivedIceCandidateFromServerListener((iceCandidate: string) => {
//       let candidate: RTCIceCandidateInit = JSON.parse(iceCandidate);
//       this.peerConnection.addIceCandidate(candidate);
//       console.log("======Received Ice Candidate======")
//     })
//   }

//   async call(): Promise<void> {
//     await this.setUpConnection();
//     const offer = await this.peerConnection.createOffer(); // this will trigger AddIceCandidate
//     await this.peerConnection.setLocalDescription(offer);
//     this.isMyOffer = true;
//     this.signalRService.sendOffer(JSON.stringify(offer));
//   }

//   async answer(transaction: Transaction): Promise<void> {
//     await this.setUpConnection(transaction);
//     const answer = await this.peerConnection.createAnswer();
//     await this.peerConnection.setLocalDescription(answer);
//     transaction.answer = JSON.stringify(answer);
//     transaction.answerUsername = this.authService.appContext()!.user.email;
//     this.signalRService.sendAnswer(transaction);
//   }

//   async joinQueue(): Promise<void> {
//     this.signalRService.joinQueue();
//   }

//   public async setUpConnection(transaction?: Transaction): Promise<void> {
//     this.localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
//     this.localVideo.nativeElement.srcObject = this.localStream;
//     //sent local string to remote
//     this.localStream.getTracks().forEach(track => {
//       this.peerConnection.addTrack(track, this.localStream);
//     })

//     //monitor incomming stream
//     this.peerConnection.ontrack = (event) => {
//       console.log("Incoming remote track...");
//       event.streams[0].getTracks().forEach(track => {
//         this.remoteStream.addTrack(track);
//       })
//     };
//     this.remoteVideo.nativeElement.srcObject = this.remoteStream;

//     this.peerConnection.addEventListener('signalingstatechange', (e) => {
//       console.log(this.peerConnection.signalingState);
//     })

//     //send ice candidate when offer is created after. When offer is created the iccandidate is created.
//     this.peerConnection.addEventListener('icecandidate', e => {
//       if (e.candidate) {
//         this.signalRService.AddIceCandidate({
//           candiate: JSON.stringify(e.candidate.toJSON()),
//           email: this.authService.appContext()!.user.email,
//           isMyOffer: this.isMyOffer,
//         })
//       }
//     })
//     if (transaction) {
//       //this won't be set when called from call();
//       //will be set when we call from answerOffer()
//       // console.log(this.peerConnection.signalingState) //should be stable because no setDesc has been run yet
//       await this.peerConnection.setRemoteDescription(JSON.parse(transaction.offer))
//       // console.log(this.peerConnection.signalingState) //should be have-remote-offer, because client2 has setRemoteDesc on the offer
//     }
//   }
// }
