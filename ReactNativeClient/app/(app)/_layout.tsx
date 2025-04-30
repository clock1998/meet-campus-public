import { Text } from 'react-native';
import { Redirect, router, Stack } from 'expo-router';
import { useSession } from '@/context/AuthContext';
import { useEffect } from 'react';
import { useMutation } from '@tanstack/react-query';
import { refresh } from '@/apis/auth';


// Helper function to check if the token needs refreshing
function tokenNeedsRefresh(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      return payload.exp < currentTime;
    } catch (e) {
      return true;
    }
  }

export default function AppLayout() {
    const { userSession, isLoading, signIn, signOut } = useSession();
    const { mutate, error } = useMutation({
        mutationFn: refresh,
        onSuccess: (data) => {
          signIn(data);
          router.replace('/');
        },
        onError:(err)=>{
            signOut();
            router.replace('/login');
        }
      })
      useEffect(() => {
        // If a session exists and the token is expired or needs refresh, trigger the refresh
        if (userSession && tokenNeedsRefresh(userSession.token)) {
          mutate({
            token: userSession.token,
            refreshToken: userSession.refreshToken,
          });
        }
      }, [userSession]);
      
    // You can keep the splash screen open, or render a loading screen like we do here.
    if (isLoading) {
        return <Text>Loading...</Text>;
    }
    // Only require authentication within the (app) group's layout as users
    // need to be able to access the (auth) group and sign in again.
    if (!userSession) {
        // On web, static rendering will stop here as the user is not authenticated
        // in the headless Node process that the pages are rendered in.
        return <Redirect href="/login" />;
    }
    
    return (
        <Stack>
            <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
            <Stack.Screen name="+not-found" />
        </Stack>
    )
}