import { useState, useRef, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useImages } from '../../hooks/useImages';
import { useAlbums } from '../../hooks/useAlbums';
import { Post } from '../../components';
import './albumEdit.css';

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

const AlbumImageUpload = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { uploadImage, loading: uploadLoading, error: uploadError } = useImages();
  const { getAlbum, loading: albumLoading, error: albumError } = useAlbums();
  
  const [imageUrl, setImageUrl] = useState('');
  const [message, setMessage] = useState<string | undefined>('');
  const [uploadType, setUploadType] = useState<'file' | 'url'>('file');
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  const [album, setAlbum] = useState<any>(null);

  if (!id) {
    navigate('/albums');
    return null;
  }

  useEffect(() => {
    const fetchAlbum = async () => {
      const albumData = await getAlbum(parseInt(id));
      setAlbum(albumData);
    };
    fetchAlbum();
  }, [id]);
  
  if (albumLoading) {
    return <div>Hleð inn albúmi...</div>;
  }

  if (albumError || !album) {
    return <div>Villa kom upp við að sækja albúm</div>;
  }

  const handleImageUrlChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setImageUrl(event.target.value);
  };

  const validateFile = (file: File): boolean => {
    if (file.size > MAX_FILE_SIZE) {
      setMessage(`Mynd er of stór. Hámarksstærð er ${MAX_FILE_SIZE / 1024 / 1024}MB`);
      return false;
    }
    
    if (!file.type.startsWith('image/')) {
      setMessage('Aðeins er hægt að hlaða upp myndum');
      return false;
    }
    
    return true;
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files) {
      // Validate all files
      const validFiles = Array.from(files).every(validateFile);
      if (validFiles) {
        setSelectedFiles(files);
        setMessage(undefined);
      } else {
        if (fileInputRef.current) {
          fileInputRef.current.value = '';
        }
        setSelectedFiles(null);
      }
    }
  };

  const handleUpload = async (event: React.FormEvent) => {
    event.preventDefault();
    setMessage('');
    setUploadProgress(0);

    try {
      if (uploadType === 'file' && selectedFiles && selectedFiles.length > 0) {
        // Upload each selected file
        for (let i = 0; i < selectedFiles.length; i++) {
          const file = selectedFiles[i];
          await uploadImage({
            file,
            name: album.name,
            albumId: album.id
          });
          setUploadProgress(((i + 1) / selectedFiles.length) * 100);
        }
        setMessage('Myndum hefur verið hlaðið upp!');
        setSelectedFiles(null);
        if (fileInputRef.current) {
          fileInputRef.current.value = '';
        }
      } else if (uploadType === 'url' && imageUrl) {
        await uploadImage({
          source: imageUrl,
          name: album.name,
          albumId: album.id
        });
        setMessage('Myndum hefur verið hlaðið upp!');
        setImageUrl('');
      }
    } catch (err) {
      console.error('Error uploading image:', err);
    }
  };

  const handleDone = () => {
    navigate(`/albums/${id}`);
  };

  return (
    <div id="main">
      <Post
        title="Bæta við myndum"
        description={`Hlaða upp myndum í ${album ? album.name : 'albúm'}`}
        body={[
          <section key="upload">
            <div className="row gtr-uniform">
              <div className="col-12">
                <select 
                  value={uploadType} 
                  onChange={(e) => setUploadType(e.target.value as 'file' | 'url')}
                  className="form-select"
                >
                  <option value="file">Velja myndir af tölvu</option>
                  <option value="url">Slá inn vefslóð</option>
                </select>
              </div>
            </div>

            <form onSubmit={handleUpload}>
              <div className="row gtr-uniform">
                {uploadType === 'file' ? (
                  <div className="col-12">
                    <input
                      type="file"
                      accept="image/*"
                      multiple
                      onChange={handleFileChange}
                      ref={fileInputRef}
                      className="form-input"
                    />
                    {uploadProgress > 0 && (
                      <div className="progress-bar">
                        <div 
                          className="progress" 
                          style={{ width: `${uploadProgress}%` }}
                        />
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="col-12">
                    <input
                      type="text"
                      value={imageUrl}
                      onChange={handleImageUrlChange}
                      placeholder="Slóð á mynd"
                      className="form-input"
                    />
                  </div>
                )}

                <div className="col-12">
                  <button 
                    className="button" 
                    type="submit"
                    disabled={uploadLoading || (uploadType === 'file' ? !selectedFiles?.length : !imageUrl)}
                  >
                    {uploadLoading ? 'Hleð upp...' : 'Hlaða upp mynd(um)'}
                  </button>
                </div>
              </div>
            </form>

            {uploadError && (
              <div className="error-message">
                {uploadError}
              </div>
            )}
            
            {message && (
              <div className="success-message">
                {message}
              </div>
            )}

            <div className="row gtr-uniform" style={{ marginTop: '2em' }}>
              <div className="col-12">
                <button className="button large" onClick={handleDone}>
                  Klára
                </button>
              </div>
            </div>
          </section>
        ]}
      />
    </div>
  );
};

export default AlbumImageUpload;