import { Text, View, StyleSheet } from 'react-native';
import { Link } from 'expo-router'; 
import ImageViewer from '@/components/ImageViewer';
import Button from '@/components/Button';
import { useSession } from '@/context/AuthContext';
export default function Index() {
  const { userSession } = useSession();
  return (
    <View style={styles.container}>
      <View style={styles.imageContainer}>
        {/* <ImageViewer imgSource={{uri: "https://picsum.photos/200"}} /> */}
        <Text>{userSession?.user.firstName}</Text>
        <Text>{userSession?.user.lastName}</Text>
      </View>
      <View style={styles.footerContainer}>
        <Button theme="primary" label="Choose a photo" />
        <Button label="Use this photo" />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: {
    color: '#fff',
  },
  button: {
    fontSize: 20,
    textDecorationLine: 'underline',
    color: '#fff',
  },
  imageContainer: {
    flex: 1,
  },
  footerContainer: {
    flex: 1 / 3,
    alignItems: 'center',
  },
});
