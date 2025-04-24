namespace PogromcaBossa.LocalLibrary;

public class Audio
{
    public async static Task PlaySound()
    {
        System.Media.SoundPlayer player = new()
        {
            SoundLocation = "D:\\Microsoft Visual Studio 2022\\ImportantProjects\\Bossa\\PogromcaBossa\\Miscellaneous\\waterdrop.wav"
        };

        player.Play();
    }
}
