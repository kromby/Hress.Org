import './albumEdit.css';
import { useState, FormEvent } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Post } from '../../components';
import DatePicker from 'react-datepicker';
import { useAuth } from '../../context/auth';
import { useAlbums } from '../../hooks/useAlbums';
import { Album } from '../../types/album';
import axios from 'axios';

const AlbumEdit: React.FC = () => {
    const { authTokens } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const { createAlbum, loading, error } = useAlbums();
    
    const [albumDate, setAlbumDate] = useState<Date>(new Date());
    const [name, setName] = useState<string>("");
    const [description, setDescription] = useState<string>("");
    const [saved, setSaved] = useState<boolean>(false);

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();

        try {
            const albumData: Album = {
                id: 0,
                name,
                description,
                date: albumDate
            };

            const album = await createAlbum(albumData);
            setSaved(true);
            navigate(`/albums/${album.id}/upload`);
        } catch (error) {
            console.error(error);
        }
    };

    if (authTokens === undefined) {
        navigate("/login", { state: { from: location.pathname } });
        return null;
    }

    return (
        <div id="main">
            <Post
                title="Nýtt albúm"
                description="Búðu til myndaalbúm"
                body={[
                    <section key="create">
                        <form onSubmit={handleSubmit}>
                            <div className="row gtr-uniform">
                                <div className="col-6">
                                    <input 
                                        type="text"
                                        placeholder="Nafn"
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
                                        required
                                    />
                                </div>
                                <div className="col-6 col-12-xsmall">
                                    <DatePicker 
                                        selected={albumDate} 
                                        onChange={(date: Date | null) => date && setAlbumDate(date)} 
                                        dateFormat="dd.MM.yyyy"
                                        className="datepicker-input"
                                    />
                                </div>
                                <div className="col-12">
                                    <textarea
                                        rows={3}
                                        value={description}
                                        onChange={(e) => setDescription(e.target.value)}
                                        placeholder="Lýsing"
                                    />
                                </div>
                                <div className="col-12">
                                    {error && <div className="error">Gat ekki vistað albúm: {error}</div>}
                                    {saved && <b>Album created!<br /></b>}
                                    <button 
                                        className="button large" 
                                        disabled={loading || !name}
                                        type="submit"
                                    >
                                        {loading ? 'Er að vista...' : 'Vista albúm'}
                                    </button>
                                </div>
                            </div>
                        </form>
                    </section>
                ]}
            />
        </div>
    );
};

export default AlbumEdit; 