import { useContext, createContext, type PropsWithChildren } from 'react';
import { useStorageState } from './UseStorageState';


export interface UserSession {
    refreshToken: string;
    token: string;
    user: User;
}
export interface User{
    email:string;
    firstName:string;
    lastName:string;
    id:string;
    roles:string[];
}
const AuthContext = createContext<{
    signIn: (data: UserSession) => void;
    signOut: () => void;
    userSession: UserSession | null;
    isLoading: boolean;
}>({
    signIn: (data:UserSession) => null,
    signOut: () => null,
    userSession: null,
    isLoading: false,
});

// This hook can be used to access the user info.
export function useSession() {
    const value = useContext(AuthContext);
    if (process.env.NODE_ENV !== 'production') {
        if (!value) {
            throw new Error('useSession must be wrapped in a <SessionProvider />');
        }
    }

    return value;
}

export function SessionProvider({ children }: PropsWithChildren) {
    const [[isLoading, session], setSession] = useStorageState('session');
    let userSession:UserSession|null=null; 
    if(session)
        userSession = JSON.parse(session) as UserSession;
    return (
        <AuthContext.Provider
            value={{
                signIn: (data: UserSession) => {
                    // Perform sign-in logic here
                    setSession(JSON.stringify(data));           
                },
                signOut: () => {
                    setSession(null);
                },
                userSession,
                isLoading,
            }}>
            {children}
        </AuthContext.Provider>
    );
}
