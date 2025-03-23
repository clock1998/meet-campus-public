import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from "@hookform/resolvers/zod"
import { Text, View, StyleSheet, TextInput, Button, KeyboardAvoidingView, Image } from 'react-native';
import { z } from "zod";
import { useSession } from '@/context/AuthContext';
import { router } from 'expo-router';
import { useMutation } from '@tanstack/react-query';
import { ApiError, login } from '@/apis/auth';
import { SafeAreaView } from 'react-native-safe-area-context';

const logo = require("../assets/images/logo_transparent.png")
const schema = z.object({
  username: z.string().email(),
  password: z.string().min(8, { message: "At least 8 characters" })
})
export default function LoginScreen() {
  const { signIn } = useSession();
  const { control, handleSubmit, formState: { errors }, } = useForm({ resolver: zodResolver(schema) })
  const { mutate: loginMutation, error: loginError, } = useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      signIn(data);
      router.replace('/');
    }
  })
  function onSumbit(data: typeof schema._type) {
    try {
      loginMutation(data);  
    } catch (error) {
      console.error(error)
    }
  }
  return (
    <KeyboardAvoidingView behavior='padding' style={styles.container}>
      <SafeAreaView>
        <Image source={logo} style={styles.imageContainer}></Image>
        <View style={styles.form}>
          <Text style={styles.label}>Username</Text>
          <Controller control={control}
            name="username"
            render={({ field: { onChange, onBlur, value } }) => (
              <TextInput style={styles.input}
                onBlur={onBlur}
                onChangeText={onChange}
                value={value} />
            )} />
            {errors.username && <Text>{errors.username.message}</Text>}
          <Text style={styles.label}>Password</Text>
          <Controller control={control}
            name="password"
            render={({ field: { onChange, onBlur, value } }) => (
              <TextInput style={styles.input}
                secureTextEntry
                onBlur={onBlur}
                onChangeText={onChange}
                value={value} />
            )} />
          {errors.password && <Text>{errors.password.message}</Text>}
          {loginError && <Text>{(loginError as ApiError).detail}</Text>}
          <Button title="Login" onPress={handleSubmit(onSumbit)}></Button>
        </View>
      </SafeAreaView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'white',
    justifyContent: 'center',
    padding: 20,
  },
  imageContainer: {
    width: 150, 
    height: 150, 
    alignSelf: 'center', 
    marginBottom: 20
  },
  form: {
    backgroundColor: "white",
  },
  label: {
    fontSize: 16,
    marginBottom: 5,
    fontWeight: "bold",
  },
  input: {
    height: 40,
    borderColor: "#ddd",
    borderWidth: 1,
    marginBottom: 15,
    padding: 10,
    borderRadius: 5,
  }
});
