using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Photon.Pun;

public class ProductInfo
{
    public string name;
    public Color color;
    public List<string> machines;
    public int coef;
    public GameObject ui;
    public bool visible = false;
}

public class ProductFlowChart : MonoBehaviour
{
    private List<ProductInfo> ProductInfos { get; set; } = new List<ProductInfo>()
    {
        new ProductInfo(){ name = "A", color = Color.red, machines = new List<string>{ "F1", "F2", "T1", "P1", "F3", "G1" }, coef =  1 },
        new ProductInfo(){ name = "B", color = Color.yellow, machines =  new List<string>{ "G2", "T3", "F2", "F4", "P2" }, coef = 5 },
        new ProductInfo(){ name = "C", color = Color.black, machines = new List<string>{ "F3", "T2", "G1", "F4", "G2" }, coef = 3 },
        new ProductInfo(){ name = "D", color = Color.blue, machines = new List<string>{ "F1", "T1", "F3", "F2", "G2" }, coef = 2 },
        new ProductInfo(){ name = "E", color = Color.cyan, machines = new List<string>{ "F3", "G1", "F4", "P3" }, coef = 3 }
    };

    private List<(List<string> path, GameObject obj, string type)> CurrentProducts = new List<(List<string> path, GameObject, string)>();

    public float speed;

    public GameObject ProdutObject;
    private GameObject ProductPanel;
    public GameObject ProductInterface;
    private Dictionary<string, bool> ProductVisibility = new Dictionary<string, bool>() {
        { "A", true },
        { "B", true },
        { "C", true },
        { "D", true },
        { "E", true },
    };

    // Start is called before the first frame update
    void Start()
    {
        ProductPanel = GameObject.Find("ProductPanel");

        ProductInfos = ProductInfos.Select(pInfo =>
        {
            StartCoroutine(CreateProduct(pInfo));
            GameObject pInterface = Instantiate(ProductInterface, ProductPanel.transform);
            pInfo.ui = pInterface;

            pInterface.transform.Find("VisibilityButton").GetComponent<Image>().color = pInfo.color;
            pInterface.transform.Find("VisibilityButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                ProductVisibility[pInfo.name] = !ProductVisibility[pInfo.name];

                if(!ProductVisibility[pInfo.name])
                {
                    foreach (var p in CurrentProducts.Where(p => p.type == pInfo.name))
                    {
                        Destroy(p.obj);
                        ProductCountChange(p.type, -1);
                    }
                    CurrentProducts = CurrentProducts.Where(p => p.type != pInfo.name).ToList();
                }

                pInterface.transform.Find("VisibilityButton").transform.Find("VisibilityLabel").GetComponent<Text>().text = ProductVisibility[pInfo.name] ? "Hide" : "Show";
            });
            pInterface.transform.Find("PathLabel").GetComponent<Text>().text = pInfo.machines.Aggregate("", (prev, next) => prev + " > " + next);
            pInterface.transform.Find("CoefLabel").GetComponent<Text>().text = pInfo.coef.ToString();

            return pInfo;
        }).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        var newList = CurrentProducts.ToList();

        foreach (var product in CurrentProducts)
        {
            GameObject target = GameObject.Find(product.path.First());
            if (target == null) continue;
            float step = speed * Time.deltaTime;
            Vector3 targetPos = target.transform.position;
            Vector3 currentPos = product.obj.transform.position;
            Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, step);
            product.obj.transform.position = newPos;

            if (Vector3.Distance(product.obj.transform.position, target.transform.position) < 0.1f)
            {
                product.path.RemoveAt(0);
                if(product.path.Count == 0)
                {
                    newList.Remove(product);
                    product.obj.GetComponent<PhotonView>().RequestOwnership();
                    Debug.Log(product.obj.GetComponent<PhotonView>().IsMine);
                    PhotonNetwork.Destroy(product.obj);
                    ProductCountChange(product.type, -1);
                }
            }
        }

        CurrentProducts = newList;
    }

    public IEnumerator CreateProduct(ProductInfo productInfo)
    {
        while (true)
        {
            yield return new WaitForSeconds(5 / productInfo.coef);

            GameObject machine = GameObject.Find(productInfo.machines.First());

            if (ProductVisibility[productInfo.name] && machine != null)
            {
                GameObject productObject = PhotonNetwork.InstantiateSceneObject("Product", machine.transform.position, new Quaternion());
                productObject.GetComponent<MeshRenderer>().material.color = productInfo.color;

                foreach (TextMesh tmesh in productObject.transform.GetComponentsInChildren<TextMesh>()) tmesh.text = productInfo.name;

                ProductCountChange(productInfo.name, 1);
                CurrentProducts.Add((productInfo.machines.ToList(), productObject, productInfo.name));
            }
        }
    }

    private void ProductCountChange(string name, int offset)
    {
        var t = ProductInfos.First(p => p.name == name).ui;
        var s = t.transform.Find("AmountLabel");
        var countLabel = ProductInfos.First(p => p.name == name).ui.transform.Find("AmountLabel").GetComponent<Text>();

        countLabel.text = (int.Parse(countLabel.text) + offset).ToString();
    }
}
