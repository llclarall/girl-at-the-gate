using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This script manages the UI system for the game, including the interaction prompt and the object display panel. It handles the opening and closing of the display panel, configuring the displayed image and text, and ensuring that the interaction prompt is shown or hidden appropriately based on the player's interactions. The script also includes functionality for trimming transparent borders from displayed images to improve their appearance in the UI. It uses a singleton pattern to allow easy access from other scripts when they need to update the UI based on player interactions.  
/// </summary>

public class UISystem : MonoBehaviour
{
    public static UISystem Instance;

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject objectPanel;

    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private bool useNativeImageSize = false;
    [SerializeField] private bool preserveImageAspect = true;
    [SerializeField] private bool normalizeImageLayout = true;
    [SerializeField] private Vector2 maxDisplayImageSize = new Vector2(900f, 700f);
    [SerializeField] private bool forceSimpleImageType = true;
    [SerializeField] private bool sizeImageFromPanel = true;
    [SerializeField][Range(0.1f, 1f)] private float panelWidthFill = 0.9f;
    [SerializeField][Range(0.1f, 1f)] private float panelHeightFill = 0.9f;
    [SerializeField] private bool trimTransparentBorders = true;
    [SerializeField][Range(0f, 1f)] private float alphaVisibilityThreshold = 0.03f;

    private Vector2 _defaultImageSize;
    private Vector2 _defaultImageAnchoredPosition;
    private bool _hasCachedImageRect;
    private readonly Dictionary<Sprite, Sprite> _trimmedSpriteCache = new Dictionary<Sprite, Sprite>();

    private bool _isObjectOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        ResolveDisplayReferences();
        CacheImageRectDefaults();
    }

    private void ResolveDisplayReferences()
    {
        if (objectPanel != null)
        {
            if (displayImage == null)
            {
                displayImage = objectPanel.GetComponentInChildren<Image>(true);
            }

            if (displayText == null)
            {
                displayText = objectPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        CacheImageRectDefaults();
    }

    private void CacheImageRectDefaults()
    {
        if (_hasCachedImageRect || displayImage == null)
        {
            return;
        }

        RectTransform imageRect = displayImage.rectTransform;
        _defaultImageSize = imageRect.sizeDelta;
        _defaultImageAnchoredPosition = imageRect.anchoredPosition;
        _hasCachedImageRect = true;
    }

    private void ConfigureDisplayImage(Sprite photo)
    {
        if (displayImage == null)
        {
            return;
        }

        RectTransform imageRect = displayImage.rectTransform;
        imageRect.localScale = Vector3.one;

        if (forceSimpleImageType)
        {
            displayImage.type = Image.Type.Simple;
        }

        displayImage.preserveAspect = preserveImageAspect;

        AspectRatioFitter aspectRatioFitter = displayImage.GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter != null)
        {
            aspectRatioFitter.enabled = false;
        }
    }

    public void ToggleInteractionPrompt(bool isActive)
    {
        if (_isObjectOpen && isActive) return;
        if (interactionPrompt != null) interactionPrompt.SetActive(isActive);
    }

    // interaction system 
    public void OpenDisplay(Sprite photo, string message)
    {
        ResolveDisplayReferences();
        _isObjectOpen = true;
        if (objectPanel != null)
        {
            objectPanel.SetActive(true);
        }
        ToggleInteractionPrompt(false);

        if (displayImage != null)
        {
            Sprite spriteToShow = ResolveSpriteForDisplay(photo);
            displayImage.sprite = spriteToShow;
            displayImage.enabled = photo != null;
            displayImage.color = new Color(1f, 1f, 1f, 1f);
            ConfigureDisplayImage(spriteToShow);

            if (photo != null)
            {
                RectTransform imageRect = displayImage.rectTransform;

                if (normalizeImageLayout && _hasCachedImageRect)
                {
                    imageRect.sizeDelta = _defaultImageSize;
                    imageRect.anchoredPosition = _defaultImageAnchoredPosition;
                }

                if (useNativeImageSize)
                {
                    displayImage.SetNativeSize();
                }

                if (normalizeImageLayout)
                {
                    FitImageInBounds(displayImage, spriteToShow, ResolveTargetImageBounds());
                }

                displayImage.gameObject.SetActive(true);
            }
        }
        if (displayText != null) displayText.text = message;
    }

    private Sprite ResolveSpriteForDisplay(Sprite source)
    {
        if (source == null || !trimTransparentBorders)
        {
            return source;
        }

        if (_trimmedSpriteCache.TryGetValue(source, out Sprite cached) && cached != null)
        {
            return cached;
        }

        Sprite trimmed = TryCreateTrimmedSprite(source);
        _trimmedSpriteCache[source] = trimmed != null ? trimmed : source;
        return _trimmedSpriteCache[source];
    }

    // Attempts to create a new sprite with transparent borders trimmed based on the alpha visibility threshold. If the source texture is not readable or an error occurs during processing, the original sprite is returned.
    private Sprite TryCreateTrimmedSprite(Sprite source)
    {
        Texture2D texture = source.texture;
        if (texture == null || !texture.isReadable)
        {
            return source;
        }

        try
        {
            Rect sourceRect = source.rect;
            int sourceXMin = Mathf.RoundToInt(sourceRect.x);
            int sourceYMin = Mathf.RoundToInt(sourceRect.y);
            int sourceWidth = Mathf.RoundToInt(sourceRect.width);
            int sourceHeight = Mathf.RoundToInt(sourceRect.height);

            Color32[] pixels = texture.GetPixels32();
            int texWidth = texture.width;
            byte alphaLimit = (byte)Mathf.RoundToInt(Mathf.Clamp01(alphaVisibilityThreshold) * 255f);

            int minX = sourceXMin + sourceWidth;
            int minY = sourceYMin + sourceHeight;
            int maxX = sourceXMin - 1;
            int maxY = sourceYMin - 1;

            for (int y = sourceYMin; y < sourceYMin + sourceHeight; y++)
            {
                int rowOffset = y * texWidth;
                for (int x = sourceXMin; x < sourceXMin + sourceWidth; x++)
                {
                    if (pixels[rowOffset + x].a <= alphaLimit)
                    {
                        continue;
                    }

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return source;
            }

            int trimmedWidth = maxX - minX + 1;
            int trimmedHeight = maxY - minY + 1;
            if (trimmedWidth <= 0 || trimmedHeight <= 0)
            {
                return source;
            }

            Vector2 sourcePivotPixels = source.pivot;
            float pivotX = Mathf.Clamp(sourcePivotPixels.x - (minX - sourceXMin), 0f, trimmedWidth);
            float pivotY = Mathf.Clamp(sourcePivotPixels.y - (minY - sourceYMin), 0f, trimmedHeight);
            Vector2 trimmedPivot = new Vector2(pivotX / trimmedWidth, pivotY / trimmedHeight);

            Sprite trimmed = Sprite.Create(
                texture,
                new Rect(minX, minY, trimmedWidth, trimmedHeight),
                trimmedPivot,
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect,
                source.border
            );

            trimmed.name = source.name + "_Trimmed";
            return trimmed;
        }
        catch
        {
            return source;
        }
    }

    // Determines the maximum size for the displayed image based on configuration and panel size
    private Vector2 ResolveTargetImageBounds()
    {
        if (sizeImageFromPanel && objectPanel != null)
        {
            RectTransform panelRect = objectPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                Rect rect = panelRect.rect;
                if (rect.width > 0f && rect.height > 0f)
                {
                    return new Vector2(rect.width * panelWidthFill, rect.height * panelHeightFill);
                }
            }
        }

        return maxDisplayImageSize;
    }

    // Scales the image to fit within the specified bounds while preserving aspect ratio
    private static void FitImageInBounds(Image image, Sprite sprite, Vector2 bounds)
    {
        if (image == null || sprite == null)
        {
            return;
        }

        if (bounds.x <= 0f || bounds.y <= 0f)
        {
            return;
        }

        Rect spriteRect = sprite.rect;
        if (spriteRect.width <= 0f || spriteRect.height <= 0f)
        {
            return;
        }

        float widthScale = bounds.x / spriteRect.width;
        float heightScale = bounds.y / spriteRect.height;
        float scale = Mathf.Min(widthScale, heightScale);

        image.rectTransform.sizeDelta = new Vector2(spriteRect.width * scale, spriteRect.height * scale);
    }

    public void CloseDisplay()
    {
        _isObjectOpen = false;
        if (objectPanel != null)
        {
            objectPanel.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyInteractionClosed();
        }
    }

    public bool IsObjectOpen()
    {
        return _isObjectOpen || (objectPanel != null && objectPanel.activeSelf);
    }
}